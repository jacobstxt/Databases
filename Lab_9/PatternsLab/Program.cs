using System.Text;
using Npgsql;
using PatternsLab;
using PatternsLab.ActiveRecord;
using PatternsLab.Benchmark;
using PatternsLab.Dao;

Console.OutputEncoding = Encoding.UTF8;

while (true)
{
    Console.WriteLine("\n=== MENU ===");
    Console.WriteLine("1 - Active Record CRUD demo");
    Console.WriteLine("2 - DAO CRUD demo");
    Console.WriteLine("3 - Benchmark (read 100 records)");
    Console.WriteLine("4 - Create test data (100 rows)");
    Console.WriteLine("5 - Delete test data");
    Console.WriteLine("0 - Exit");
    Console.Write("Choose an option: ");

    var choice = Console.ReadLine()?.Trim();
    if (choice == "0") break;

    try
    {
        switch (choice)
        {
            case "1": RunActiveRecordDemo(); break;
            case "2": RunDaoDemo(); break;
            case "3": BenchmarkRunner.RunReadBenchmark(); break;
            case "4": BenchmarkRunner.SeedTestData(); break;
            case "5": BenchmarkRunner.DeleteTestData(); break;
            default: Console.WriteLine("Unknown option, try again."); break;
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
}

static void RunActiveRecordDemo()
{
    Console.WriteLine("\n=== ACTIVE RECORD DEMO ===");


    var buyer = new Buyer
    {
        FullName = "Active Record Demo",
        Phone = "+380991110001",
        Email = "ar.demo@example.com",
        BankAccount = "UA" + new string('7', 27),
        BankId = 1
    };
    buyer.Save();
    Console.WriteLine($"CREATE: {buyer}");

    Console.WriteLine($"READ:   {Buyer.FindById(buyer.BuyerId)}");

    buyer.Phone = "+380991110002";
    buyer.Save();
    Console.WriteLine($"UPDATE: {Buyer.FindById(buyer.BuyerId)}");

    var id = buyer.BuyerId;
    buyer.Delete();
    Console.WriteLine($"DELETE: {(Buyer.FindById(id) is null ? "record removed" : "record still exists")}");
}

static void RunDaoDemo()
{
    Console.WriteLine("\n=== DAO DEMO ===");

    IBuyerDao dao = new BuyerDao();

    var model = new BuyerModel
    {
        FullName = "DAO Demo",
        Phone = "+380991110003",
        Email = "dao.demo@example.com",
        BankAccount = "UA" + new string('6', 27),
        BankId = 1
    };
    dao.Insert(model);
    Console.WriteLine($"CREATE: {model}");

    Console.WriteLine($"READ:   {dao.GetById(model.BuyerId)}");

    model.Phone = "+380991110004";
    dao.Update(model);
    Console.WriteLine($"UPDATE: {dao.GetById(model.BuyerId)}");

    var deleted = dao.Delete(model.BuyerId);
    Console.WriteLine($"DELETE: {(deleted ? "record removed" : "nothing deleted")}");
}