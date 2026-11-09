using Npgsql;

namespace DbClient;

public static class Crud
{
    public static void SelectAll(NpgsqlConnection conn)
    {
        const string sql = @"
            SELECT buyer_id, full_name, phone, email, bank_account, created_at, bank_id
            FROM buyers
            ORDER BY buyer_id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        Console.WriteLine(
            $"{"buyer_id",-9} {"full_name",-30} {"phone",-14} {"email",-24} " +
            $"{"bank_account",-30} {"created_at",-20} {"bank_id",-7}");
        Console.WriteLine(new string('-', 140));

        while (reader.Read())
        {
            Console.WriteLine(
                $"{reader.GetInt32(0),-9} {reader.GetString(1),-30} {reader.GetString(2),-14} " +
                $"{reader.GetString(3),-24} {reader.GetString(4),-30} " +
                $"{reader.GetDateTime(5):yyyy-MM-dd HH:mm:ss,-20} {reader.GetInt32(6),-7}");
        }
    }

    public static void SelectById(NpgsqlConnection conn, int id)
    {
        const string sql = @"
            SELECT buyer_id, full_name, phone, email, bank_account, bank_id
            FROM buyers
            WHERE buyer_id = @id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            Console.WriteLine(
                $"Found: {reader.GetInt32(0)} | {reader.GetString(1)} | {reader.GetString(2)} | " +
                $"{reader.GetString(3)} | {reader.GetString(4)} | bank_id = {reader.GetInt32(5)}");
        }
        else
        {
            Console.WriteLine($"No buyer with id = {id}.");
        }
    }

    public static int Insert(
        NpgsqlConnection conn,
        string fullName, string phone, string email, string bankAccount, int bankId)
    {
        const string sql = @"
            INSERT INTO buyers (full_name, phone, email, bank_account, bank_id)
            VALUES (@name, @phone, @email, @account, @bank)
            RETURNING buyer_id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", fullName);
        cmd.Parameters.AddWithValue("@phone", phone);
        cmd.Parameters.AddWithValue("@email", email);
        cmd.Parameters.AddWithValue("@account", bankAccount);
        cmd.Parameters.AddWithValue("@bank", bankId);

        return (int)cmd.ExecuteScalar()!;
    }

    public static int UpdatePhone(NpgsqlConnection conn, int id, string newPhone)
    {
        const string sql = "UPDATE buyers SET phone = @phone WHERE buyer_id = @id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@phone", newPhone);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery();
    }

    public static int Delete(NpgsqlConnection conn, int id)
    {
        const string sql = "DELETE FROM buyers WHERE buyer_id = @id;";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery();
    }
}