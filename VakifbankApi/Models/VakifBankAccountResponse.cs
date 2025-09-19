namespace VakifBankApi.Models
{
	public class VakifBankAccountResponse
	{
		public VakifBankHeader? Header { get; set; }
		public VakifBankAccountData? Data { get; set; }
	}
	public class VakifBankHeader
	{
		public string? StatusCode { get; set; }
		public string? StatusDescription { get; set; }
		public string? StatusDescriptionEn { get; set; }
		public string? ObjectID { get; set; }
	}
	public class VakifBankAccountData
	{
		public List<Account>? Accounts { get; set; }
	}

	public class Account
	{
		public string? CurrencyCode { get; set; }
		public string? LastTransactionDate { get; set; }
		public string? AccountStatus { get; set; }
		public string? IBAN { get; set; }
		public string? RemainingBalance { get; set; }
		public string? Balance { get; set; }
		public string? AccountType { get; set; }
		public string? AccountNumber { get; set; }
	}
}