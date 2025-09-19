using System.Text.Json.Serialization; // Gerekirse

namespace VakifBankApi.Models
{
    // Bankanýn accountDetail endpoint'i için yanýt modeli
    public class VakifBankAccountDetailResponse
    {
        public VakifBankHeader? Header { get; set; } // Ortak Header modeli
        public VakifBankAccountDetailData? Data { get; set; }
    }

    public class VakifBankAccountDetailData
    {
        public AccountInfo? AccountInfo { get; set; } // Burasý AccountInfo oldu
    }

    public class AccountInfo // Yeni AccountInfo sýnýfý
    {
        public string? CurrencyCode { get; set; }
        public string? LastTransactionDate { get; set; } 
        public string? AccountStatus { get; set; }       
        public string? OpeningDate { get; set; }         
        public string? IBAN { get; set; }
        public string? CustomerNumber { get; set; }      
        public string? RemainingBalance { get; set; }    
        public string? Balance { get; set; }
        public string? AccountType { get; set; }         
        public string? BranchCode { get; set; }          
        public string? AccountNumber { get; set; }
        
    }
}