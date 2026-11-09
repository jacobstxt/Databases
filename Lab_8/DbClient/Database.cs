using Npgsql;

namespace DbClient;

public record ColumnInfo(
       string Name,
       string DataType,
       int? MaxLength,
       bool IsNullable,
       string? DefaultValue,
       bool IsPrimaryKey);

public static class Database
{
    private static string GetConnectionString() =>
       Environment.GetEnvironmentVariable("DB_CONNECTION")
       ?? throw new InvalidOperationException(
           "Environment variable DB_CONNECTION is not set.");

    public static NpgsqlConnection Connect()
    {
        var conn = new NpgsqlConnection(GetConnectionString());
        conn.Open();
        return conn;
    }

    public static List<string> GetTables(NpgsqlConnection conn)
    {
        var tables = new List<string>();

        const string sql = @"
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = @schema
              AND table_type = 'BASE TABLE'
            ORDER BY table_name;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@schema", "public");

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }


    public static List<ColumnInfo> GetTableStructure(NpgsqlConnection conn, string tableName)
    {
        var columns = new List<ColumnInfo>();

        const string sql = @"
            SELECT c.column_name,
                   c.data_type,
                   c.character_maximum_length,
                   c.is_nullable,
                   c.column_default,
                   EXISTS (
                       SELECT 1
                       FROM information_schema.table_constraints tc
                       JOIN information_schema.key_column_usage kcu
                         ON tc.constraint_name = kcu.constraint_name
                        AND tc.table_schema = kcu.table_schema
                       WHERE tc.constraint_type = 'PRIMARY KEY'
                         AND tc.table_schema = c.table_schema
                         AND tc.table_name = c.table_name
                         AND kcu.column_name = c.column_name
                   ) AS is_pk
            FROM information_schema.columns c
            WHERE c.table_schema = @schema
              AND c.table_name = @table
            ORDER BY c.ordinal_position;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@schema", "public");
        cmd.Parameters.AddWithValue("@table", tableName);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            columns.Add(new ColumnInfo(
                Name: reader.GetString(0),
                DataType: reader.GetString(1),
                MaxLength: reader.IsDBNull(2) ? null : reader.GetInt32(2),
                IsNullable: reader.GetString(3) == "YES",
                DefaultValue: reader.IsDBNull(4) ? null : reader.GetString(4),
                IsPrimaryKey: reader.GetBoolean(5)));
        }

        return columns;
    }
}