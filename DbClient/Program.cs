using DbClient;
using Npgsql;

try
{
    using var conn = Database.Connect();
    Console.WriteLine($"Connected to database '{conn.Database}' at {conn.Host}:{conn.Port}");

    Console.WriteLine("\nTables in the database:");
    var tables = Database.GetTables(conn);

    if (tables.Count == 0)
    {
        Console.WriteLine("  (no tables found)");
    }
    else
    {
        foreach (var table in tables)
        {
            Console.WriteLine($"  - {table}");
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