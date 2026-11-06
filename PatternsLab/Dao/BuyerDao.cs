using Npgsql;

namespace PatternsLab.Dao;

public class BuyerDao : IBuyerDao
{
    public int Insert(BuyerModel buyer)
    {
        using var conn = Database.Connect();

        const string sql = @"
            INSERT INTO buyers (full_name, phone, email, bank_account, bank_id)
            VALUES (@name, @phone, @email, @account, @bank)
            RETURNING buyer_id, created_at;";

        using var cmd = new NpgsqlCommand(sql, conn);
        AddValues(cmd, buyer);

        using var reader = cmd.ExecuteReader();
        reader.Read();
        buyer.BuyerId = reader.GetInt32(0);
        buyer.CreatedAt = reader.GetDateTime(1);
        return buyer.BuyerId;
    }

    public BuyerModel? GetById(int id)
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

    public List<BuyerModel> GetAll(int? limit = null)
    {
        using var conn = Database.Connect();

        var sql = @"
            SELECT buyer_id, full_name, phone, email, bank_account, bank_id, created_at
            FROM buyers
            ORDER BY buyer_id";
        if (limit.HasValue) sql += " LIMIT @limit";

        using var cmd = new NpgsqlCommand(sql, conn);
        if (limit.HasValue) cmd.Parameters.AddWithValue("@limit", limit.Value);

        var result = new List<BuyerModel>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(FromReader(reader));
        }
        return result;
    }

    public bool Update(BuyerModel buyer)
    {
        using var conn = Database.Connect();

        const string sql = @"
            UPDATE buyers
            SET full_name = @name, phone = @phone, email = @email,
                bank_account = @account, bank_id = @bank
            WHERE buyer_id = @id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        AddValues(cmd, buyer);
        cmd.Parameters.AddWithValue("@id", buyer.BuyerId);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int id)
    {
        using var conn = Database.Connect();

        const string sql = "DELETE FROM buyers WHERE buyer_id = @id;";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    private static void AddValues(NpgsqlCommand cmd, BuyerModel b)
    {
        cmd.Parameters.AddWithValue("@name", b.FullName);
        cmd.Parameters.AddWithValue("@phone", b.Phone);
        cmd.Parameters.AddWithValue("@email", b.Email);
        cmd.Parameters.AddWithValue("@account", b.BankAccount);
        cmd.Parameters.AddWithValue("@bank", b.BankId);
    }

    private static BuyerModel FromReader(NpgsqlDataReader r) => new()
    {
        BuyerId = r.GetInt32(0),
        FullName = r.GetString(1),
        Phone = r.GetString(2),
        Email = r.GetString(3),
        BankAccount = r.GetString(4),
        BankId = r.GetInt32(5),
        CreatedAt = r.GetDateTime(6)
    };
}