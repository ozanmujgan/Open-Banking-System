namespace VakifBankApi.Models
{
    // Bankan�n accountDetail endpoint'i i�in istek modeli
    // Genellikle hesap numaras� gibi bir parametre al�r.
    public class VakifBankAccountDetailRequest
    {
        public string? AccountNumber { get; set; }
        // Di�er olas� alanlar buraya eklenebilir (�rne�in IBAN, CurrencyCode vb.)
    }
}