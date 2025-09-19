namespace VakifBankApi.Models 
{
    public class BankTransaction
    {
        public int Id { get; set; } // Primary Key
        public int BankAccountId { get; set; } // Foreign Key: Hangi hesaba ait

        public decimal Amount { get; set; } // Ýþlem miktarý
        public string TransactionType { get; set; } = string.Empty; // Ýþlem türü (Para Yatýrma, Çekme, Havale)
        public string Description { get; set; } = string.Empty; // Ýþlem açýklamasý
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow; // Ýþlem tarihi ve saati
        // public BankAccount? BankAccount { get; set; } // Navigasyon özelliði
    }
}