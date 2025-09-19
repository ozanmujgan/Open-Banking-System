namespace VakifBankApi.Models
{
    // Bankanýn accountDetail endpoint'i için istek modeli
    // Genellikle hesap numarasý gibi bir parametre alýr.
    public class VakifBankAccountDetailRequest
    {
        public string? AccountNumber { get; set; }
        // Diðer olasý alanlar buraya eklenebilir (örneðin IBAN, CurrencyCode vb.)
    }
}