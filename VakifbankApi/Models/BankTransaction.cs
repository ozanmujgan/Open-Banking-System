namespace VakifBankApi.Models 
{
    public class BankTransaction
    {
        public int Id { get; set; } // Primary Key
        public int BankAccountId { get; set; } // Foreign Key: Hangi hesaba ait

        public decimal Amount { get; set; } // ��lem miktar�
        public string TransactionType { get; set; } = string.Empty; // ��lem t�r� (Para Yat�rma, �ekme, Havale)
        public string Description { get; set; } = string.Empty; // ��lem a��klamas�
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow; // ��lem tarihi ve saati
        // public BankAccount? BankAccount { get; set; } // Navigasyon �zelli�i
    }
}