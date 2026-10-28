using DbClient;
using Microsoft.VisualBasic;
using Npgsql;
using System.Text;


Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

try
{
    using var conn = Database.Connect();
    Console.WriteLine($"Connected to database '{conn.Database}' at {conn.Host}:{conn.Port}");

    while (true)
    {
        Console.WriteLine("\n=== MENU ===");
        Console.WriteLine("1 - List all tables");
        Console.WriteLine("2 - Show table structure");
        Console.WriteLine("3 - Run predefined CRUD demo");
        Console.WriteLine("4 - Run custom SQL query");
        Console.WriteLine("0 - Exit");
        Console.Write("Choose an option: ");

        switch (Console.ReadLine()?.Trim())
        {
            case "1": ShowTables(conn); break;
            case "2": ShowStructure(conn); break;
            case "3": RunCrudDemo(conn); break;
            case "4": Interactive.RunCustomQuery(conn); break;
            case "0": return;
            default: Console.WriteLine("Unknown option, try again."); break;
        }
    }
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
}
catch (NpgsqlException ex)
{
    Console.WriteLine($"Database error: {ex.Message}");
}


static void ShowTables(NpgsqlConnection conn)
{
    Console.WriteLine("\nTables in the database:");
    var tables = Database.GetTables(conn);
    if (tables.Count == 0) Console.WriteLine("  (no tables found)");
    foreach (var table in tables) Console.WriteLine($"  - {table}");
}


static void ShowStructure(NpgsqlConnection conn)
{
    Console.Write("Enter a table name: ");
    var tableName = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(tableName))
    {
        Console.WriteLine("Table name cannot be empty.");
        return;
    }

    var columns = Database.GetTableStructure(conn, tableName);
    if (columns.Count == 0)
    {
        Console.WriteLine($"Table '{tableName}' was not found.");
        return;
    }

    Console.WriteLine($"\nStructure of table '{tableName}':");
    Console.WriteLine($"{"Column",-20} {"Type",-25} {"Nullable",-9} {"Key",-4} Default");
    Console.WriteLine(new string('-', 80));
    foreach (var col in columns)
    {
        var type = col.MaxLength.HasValue ? $"{col.DataType}({col.MaxLength})" : col.DataType;
        Console.WriteLine(
            $"{col.Name,-20} {type,-25} {(col.IsNullable ? "YES" : "NO"),-9} " +
            $"{(col.IsPrimaryKey ? "PK" : ""),-4} {col.DefaultValue}");
    }
}


static void RunCrudDemo(NpgsqlConnection conn)
{
    Console.WriteLine("\n=== SELECT (all rows) ===");
    Crud.SelectAll(conn);

    Console.WriteLine("\n=== INSERT ===");
    var newId = Crud.Insert(conn, "Test Client Demo", "+380991112233",
        "demo.client@example.com", "UA" + new string('9', 27), 1);
    Console.WriteLine($"Inserted a new buyer with id = {newId}.");
    Crud.SelectById(conn, newId);

    Console.WriteLine("\n=== UPDATE ===");
    Console.WriteLine($"Rows updated: {Crud.UpdatePhone(conn, newId, "+380990000999")}.");
    Crud.SelectById(conn, newId);

    Console.WriteLine("\n=== DELETE ===");
    Console.WriteLine($"Rows deleted: {Crud.Delete(conn, newId)}.");
    Crud.SelectById(conn, newId);
}