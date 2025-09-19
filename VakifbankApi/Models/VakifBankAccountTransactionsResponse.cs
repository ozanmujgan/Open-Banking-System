using System.Collections.Generic;
using System.Text.Json.Serialization; // JsonSerializerOptions { PropertyNameCaseInsensitive = true } kullan�ld��� i�in gerekmeyebilir ancak emin olmak i�in ekleyebiliriz

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
        public string? TransactionType { get; set; } // 1: Hesaba giren para, 2: Hesaptan ��kan para
        public string? Description { get; set; }
        public string? Amount { get; set; }       // �rnek yan�tta string olarak geldi�i i�in string kulland�k. "650.0" gibi.
        public string? TransactionCode { get; set; }
        public string? Balance { get; set; }      // �rnek yan�tta string olarak geldi�i i�in string kulland�k. "4957.61" gibi.
        public string? TransactionName { get; set; }
        public string? TransactionDate { get; set; } // yyyy-MM-ddTHH:mm:ss format�nda bekleniyor
        public string? TransactionId { get; set; }
    }
}