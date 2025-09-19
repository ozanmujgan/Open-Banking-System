namespace VakifBankApi.Models;

public class MyBankTransactionsRequest
{
    public int BankAccountId { get; set; }
    public DateTime? StartDate { get; set; } // UTC bekliyoruz (toISOString())
    public DateTime? EndDate { get; set; }
}
