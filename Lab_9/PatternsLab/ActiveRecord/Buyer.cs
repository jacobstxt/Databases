using Npgsql;

namespace PatternsLab.ActiveRecord;

public class Buyer
{
    public int BuyerId { get; private set; }   
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string BankAccount { get; set; } = "";
    public int BankId { get; set; }
    public DateTime CreatedAt { get; private set; }  
    public bool IsPersisted => BuyerId != 0;
    public void Save()
    {
        using var conn = Database.Connect();

        if (!IsPersisted)
        {
            const string sql = @"
                INSERT INTO buyers (full_name, phone, email, bank_account, bank_id)
                VALUES (@name, @phone, @email, @account, @bank)
                RETURNING buyer_id, created_at;";

            using var cmd = new NpgsqlCommand(sql, conn);
            AddValues(cmd);

            using var reader = cmd.ExecuteReader();
            reader.Read();
            BuyerId = reader.GetInt32(0);
            CreatedAt = reader.GetDateTime(1);
        }
        else
        {
            const string sql = @"
                UPDATE buyers
                SET full_name = @name, phone = @phone, email = @email,
                    bank_account = @account, bank_id = @bank
                WHERE buyer_id = @id;";

            using var cmd = new NpgsqlCommand(sql, conn);
            AddValues(cmd);
            cmd.Parameters.AddWithValue("@id", BuyerId);
            cmd.ExecuteNonQuery();
        }
    }

    public void Delete()
    {
        if (!IsPersisted)
            throw new InvalidOperationException("Cannot delete a buyer that is not saved yet.");

        using var conn = Database.Connect();

        const string sql = "DELETE FROM buyers WHERE buyer_id = @id;";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", BuyerId);
        cmd.ExecuteNonQuery();

        BuyerId = 0;
    }

    public static Buyer? FindById(int id)
    {
        using var conn = Database.Connect();

        const string sql = @"
            SELECT buyer_id, full_name, phone, email, bank_account, bank_id, created_at
            FROM buyers
            WHERE buyer_id = @id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? FromReader(reader) : null;
    }

    public static List<Buyer> FindAll(int? limit = null)
    {
        using var conn = Database.Connect();

        var sql = @"
            SELECT buyer_id, full_name, phone, email, bank_account, bank_id, created_at
            FROM buyers
            ORDER BY buyer_id";
        if (limit.HasValue) sql += " LIMIT @limit";

        using var cmd = new NpgsqlCommand(sql, conn);
        if (limit.HasValue) cmd.Parameters.AddWithValue("@limit", limit.Value);

        var result = new List<Buyer>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(FromReader(reader));
        }
        return result;
    }

    private void AddValues(NpgsqlCommand cmd)
    {
        cmd.Parameters.AddWithValue("@name", FullName);
        cmd.Parameters.AddWithValue("@phone", Phone);
        cmd.Parameters.AddWithValue("@email", Email);
        cmd.Parameters.AddWithValue("@account", BankAccount);
        cmd.Parameters.AddWithValue("@bank", BankId);
    }

    private static Buyer FromReader(NpgsqlDataReader r) => new()
    {
        BuyerId = r.GetInt32(0),
        FullName = r.GetString(1),
        Phone = r.GetString(2),
        Email = r.GetString(3),
        BankAccount = r.GetString(4),
        BankId = r.GetInt32(5),
        CreatedAt = r.GetDateTime(6)
    };

    public override string ToString() =>
        $"[{BuyerId}] {FullName} | {Phone} | {Email} | {BankAccount} | bank_id={BankId}";
}