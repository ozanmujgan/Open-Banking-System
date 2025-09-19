namespace VakifBankApi.Enums 
{
    public enum RegisterResult
    {
        Success,              // Kayýt iþlemi baþarýyla tamamlandý
        UsernameAlreadyExists, // Belirtilen kullanýcý adý zaten sistemde mevcut
        PasswordLengthInvalid  // Þifre belirlenen uzunluk kriterini karþýlamýyor
    }
}