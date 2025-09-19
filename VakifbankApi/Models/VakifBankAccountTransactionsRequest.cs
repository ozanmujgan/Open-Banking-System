namespace VakifBankApi.Models
{
    public class VakifBankAccountTransactionsRequest
    {
        public string? AccountNumber { get; set; } // Zorunlu
        public string? StartDate { get; set; }     // Zorunlu (yyyy-MM-ddTHH24:mm:ss)
        public string? EndDate { get; set; }       // Zorunlu (yyyy-MM-ddTHH24:mm:ss)
    }
}