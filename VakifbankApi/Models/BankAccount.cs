namespace VakifBankApi.Models 
{
	public class BankAccount
	{
		public int Id { get; set; } // Primary Key
		public string AccountNumber { get; set; } = string.Empty; // Benzersiz hesap numarasý
		public string IBAN { get; set; } = string.Empty;
		public decimal Balance { get; set; } // Hesap bakiyesi
		public string CurrencyCode { get; set; } = string.Empty; // Para birimi (TL, USD)
		public string AccountName { get; set; } = string.Empty; // Hesabýn adý (örn. "Ana Hesap")

		public int UserId { get; set; } // Foreign Key: Hangi kullanýcýya ait
										// public User? User { get; set; } // Navigasyon özelliði (EF Core için opsiyonel)
	}
}