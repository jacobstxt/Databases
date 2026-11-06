using System.Diagnostics;
using Npgsql;
using PatternsLab.ActiveRecord;
using PatternsLab.Dao;

namespace PatternsLab.Benchmark;

public static class BenchmarkRunner
{
    private const string TestEmailPattern = "bench%@example.com";

    public static void SeedTestData(int count = 100)
    {
        using var conn = Database.Connect();

        if (CountTestRows(conn) > 0)
        {
            Console.WriteLine("Test data already exists. Delete it first (option 5).");
            return;
        }

        const string sql = @"
            INSERT INTO buyers (full_name, phone, email, bank_account, bank_id)
            SELECT 'Bench User ' || g,
                   '+3809900' || lpad(g::text, 5, '0'),
                   'bench' || g || '@example.com',
                   'UA' || lpad(g::text, 27, '0'),
                   1
            FROM generate_series(1, @count) AS g;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@count", count);
        var inserted = cmd.ExecuteNonQuery();

        Console.WriteLine($"Inserted {inserted} test rows.");
    }

    public static void DeleteTestData()
    {
        using var conn = Database.Connect();

        const string sql = "DELETE FROM buyers WHERE email LIKE @pattern;";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@pattern", TestEmailPattern);

        Console.WriteLine($"Deleted {cmd.ExecuteNonQuery()} test rows.");
    }

    public static void RunReadBenchmark(int rows = 100, int runs = 20)
    {
        var dao = new BuyerDao();

        using (var conn = Database.Connect())
        {
            var total = CountAllRows(conn);
            if (total < rows)
            {
                Console.WriteLine($"Table has only {total} rows, need at least {rows}. Use option 4 first.");
                return;
            }
        }

        Buyer.FindAll(rows);
        dao.GetAll(rows);

        var arTimes = new double[runs];
        var daoTimes = new double[runs];
        var sw = new Stopwatch();
        int arCount = 0, daoCount = 0;

        for (int i = 0; i < runs; i++)
        {
            sw.Restart();
            arCount = Buyer.FindAll(rows).Count;
            sw.Stop();
            arTimes[i] = sw.Elapsed.TotalMilliseconds;

            sw.Restart();
            daoCount = dao.GetAll(rows).Count;
            sw.Stop();
            daoTimes[i] = sw.Elapsed.TotalMilliseconds;
        }

        Console.WriteLine($"\nRead {rows} records, {runs} runs (first warm-up run excluded)");
        Console.WriteLine($"{"Approach",-16} {"Rows",-6} {"Avg, ms",-10} {"Min, ms",-10} {"Max, ms",-10}");
        Console.WriteLine(new string('-', 54));
        PrintRow("Active Record", arCount, arTimes);
        PrintRow("DAO", daoCount, daoTimes);
    }

    private static void PrintRow(string name, int rows, double[] times) =>
        Console.WriteLine(
            $"{name,-16} {rows,-6} {times.Average(),-10:F3} {times.Min(),-10:F3} {times.Max(),-10:F3}");

    private static long CountTestRows(NpgsqlConnection conn)
    {
        using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM buyers WHERE email LIKE @pattern;", conn);
        cmd.Parameters.AddWithValue("@pattern", TestEmailPattern);
        return (long)cmd.ExecuteScalar()!;
    }

    private static long CountAllRows(NpgsqlConnection conn)
    {
        using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM buyers;", conn);
        return (long)cmd.ExecuteScalar()!;
    }
}