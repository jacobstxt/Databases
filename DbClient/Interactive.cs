using System.Text.RegularExpressions;
using Npgsql;
using NpgsqlTypes;

namespace DbClient;

public static class Interactive
{
    // Only these statement types can be executed from the console.
    private static readonly string[] AllowedStatements = { "SELECT", "INSERT", "UPDATE", "DELETE" };

    // Finds placeholders like @id or @name (ignores '::' casts and '@@').
    private static readonly Regex ParamRegex =
        new(@"(?<![\w@:])@([A-Za-z_]\w*)", RegexOptions.Compiled);

    // Reads an SQL query from the user, validates it, asks for parameter values
    // and executes it. All user values go ONLY through query parameters.
    public static void RunCustomQuery(NpgsqlConnection conn)
    {
        Console.WriteLine("Enter one SQL query (SELECT / INSERT / UPDATE / DELETE).");
        Console.WriteLine("Use placeholders like @id or @name for values - you will be asked to enter them.");
        Console.WriteLine("Example: SELECT * FROM buyers WHERE buyer_id = @id");
        Console.Write("SQL> ");
        var input = Console.ReadLine();

        // Step 1: local validation before touching the database.
        var error = Validate(input, out var sql, out var kind);
        if (error != null)
        {
            Console.WriteLine($"Invalid query: {error}");
            return;
        }

        // Step 2: collect values for every placeholder found in the query.
        var names = ParamRegex.Matches(sql).Select(m => m.Groups[1].Value).Distinct().ToList();
        var values = new Dictionary<string, string?>();
        foreach (var name in names)
        {
            Console.Write($"Value for @{name} (type NULL for NULL): ");
            var value = Console.ReadLine() ?? "";
            values[name] = value.Trim().Equals("NULL", StringComparison.OrdinalIgnoreCase) ? null : value;
        }

        // Step 3: execute inside a transaction.
        // If anything fails, the transaction is rolled back when it is disposed.
        try
        {
            using var tx = conn.BeginTransaction();
            using var cmd = new NpgsqlCommand(sql, conn, tx);

            foreach (var (name, value) in values)
            {
                // NpgsqlDbType.Unknown lets PostgreSQL infer the real column type
                // (int, date, text...), so "5" works for an integer column.
                cmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Unknown)
                {
                    Value = value is null ? DBNull.Value : value
                });
            }

            int shown = 0;
            int affected;
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.FieldCount > 0)
                {
                    shown = PrintRows(reader);
                }
                reader.Close();
                affected = reader.RecordsAffected;
            }

            if (kind == "SELECT")
            {
                Console.WriteLine($"{shown} row(s) returned.");
                tx.Commit();
                return;
            }

            if (shown > 0) Console.WriteLine($"{shown} row(s) returned by RETURNING.");
            Console.WriteLine($"Rows affected: {affected}.");
            Console.Write("Commit changes? (y/n): ");
            if (Console.ReadLine()?.Trim().ToLowerInvariant() == "y")
            {
                tx.Commit();
                Console.WriteLine("Changes committed.");
            }
            else
            {
                tx.Rollback();
                Console.WriteLine("Changes rolled back.");
            }
        }
        catch (PostgresException ex)
        {
            Console.WriteLine(Explain(ex));
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Database error: {ex.Message}");
        }
    }

    // Checks the query text. Returns an error message or null if the query is acceptable.
    private static string? Validate(string? input, out string sql, out string kind)
    {
        sql = "";
        kind = "";

        if (string.IsNullOrWhiteSpace(input))
            return "the query is empty.";

        sql = input.Trim();
        if (sql.EndsWith(';')) sql = sql[..^1].TrimEnd();

        if (sql.Contains(';'))
            return "only one statement is allowed (remove extra ';').";

        kind = sql.Split(' ', '\t', '\r', '\n')[0].ToUpperInvariant();
        if (!AllowedStatements.Contains(kind))
            return $"statement '{kind}' is not allowed. Only SELECT, INSERT, UPDATE and DELETE are supported.";

        if (kind is "UPDATE" or "DELETE" &&
            !Regex.IsMatch(sql, @"\bWHERE\b", RegexOptions.IgnoreCase))
            return $"{kind} without WHERE is not allowed (it would affect every row).";

        return null;
    }

    // Turns a PostgreSQL error into a clear message for the user.
    private static string Explain(PostgresException ex) => ex.SqlState switch
    {
        "42601" => $"Syntax error in the query (position {ex.Position}): {ex.MessageText}",
        "42P01" => $"Table does not exist: {ex.MessageText}",
        "42703" => $"Column does not exist: {ex.MessageText}",
        "42883" => $"Wrong operator or function for these types: {ex.MessageText}",
        "22P02" or "22007" or "22003" or "22008" =>
            $"Invalid value for the column type: {ex.MessageText}",
        "23502" => $"A required (NOT NULL) value is missing: {ex.MessageText}",
        "23503" => $"Foreign key violation (referenced row is missing or still in use): {ex.MessageText}",
        "23505" => $"Duplicate value violates a unique constraint: {ex.MessageText}",
        _ => $"Database error [{ex.SqlState}]: {ex.MessageText}"
    };

    // Prints the result set as a table. Returns the number of rows printed.
    private static int PrintRows(NpgsqlDataReader reader)
    {
        const int maxRows = 100;
        const int maxWidth = 30;

        var headers = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
        var rows = new List<string[]>();

        while (reader.Read() && rows.Count < maxRows)
        {
            var row = new string[reader.FieldCount];
            for (int i = 0; i < row.Length; i++)
            {
                row[i] = reader.IsDBNull(i)
                    ? "NULL"
                    : reader.GetValue(i) is DateTime dt
                        ? dt.ToString("yyyy-MM-dd HH:mm:ss")
                        : reader.GetValue(i).ToString() ?? "";
                if (row[i].Length > maxWidth) row[i] = row[i][..(maxWidth - 3)] + "...";
            }
            rows.Add(row);
        }

        var widths = headers.Select((h, i) =>
            Math.Min(maxWidth, Math.Max(h.Length, rows.Count == 0 ? 0 : rows.Max(r => r[i].Length)))).ToArray();

        Console.WriteLine(string.Join(" | ", headers.Select((h, i) => h.PadRight(widths[i]))));
        Console.WriteLine(string.Join("-+-", widths.Select(w => new string('-', w))));
        foreach (var row in rows)
            Console.WriteLine(string.Join(" | ", row.Select((v, i) => v.PadRight(widths[i]))));

        return rows.Count;
    }
}