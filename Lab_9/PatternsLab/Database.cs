using Npgsql;

namespace PatternsLab;


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
}