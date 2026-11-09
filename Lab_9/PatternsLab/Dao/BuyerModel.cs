namespace PatternsLab.Dao;

public class BuyerModel
{
    public int BuyerId { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string BankAccount { get; set; } = "";
    public int BankId { get; set; }
    public DateTime CreatedAt { get; set; }

    public override string ToString() =>
        $"[{BuyerId}] {FullName} | {Phone} | {Email} | {BankAccount} | bank_id={BankId}";
}