using Npgsql;

namespace DbClient;

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
}