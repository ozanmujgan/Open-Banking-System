using System.Collections.Generic;
using System.Text.Json.Serialization; // JsonSerializerOptions { PropertyNameCaseInsensitive = true } kullanýldýðý için gerekmeyebilir ancak emin olmak için ekleyebiliriz

namespace VakifBankApi.Models
{
    public class VakifBankAccountTransactionsResponse
    {
        public VakifBankHeader? Header { get; set; } // Ortak Header modeli
        public VakifBankAccountTransactionsData? Data { get; set; }
    }

    public class VakifBankAccountTransactionsData
    {
        public List<AccountTransaction>? AccountTransactions { get; set; }
    }

    public class AccountTransaction
    {
        public string? CurrencyCode { get; set; }
        public string? TransactionType { get; set; } // 1: Hesaba giren para, 2: Hesaptan çýkan para
        public string? Description { get; set; }
        public string? Amount { get; set; }       // Örnek yanýtta string olarak geldiði için string kullandýk. "650.0" gibi.
        public string? TransactionCode { get; set; }
        public string? Balance { get; set; }      // Örnek yanýtta string olarak geldiði için string kullandýk. "4957.61" gibi.
        public string? TransactionName { get; set; }
        public string? TransactionDate { get; set; } // yyyy-MM-ddTHH:mm:ss formatýnda bekleniyor
        public string? TransactionId { get; set; }
    }
}