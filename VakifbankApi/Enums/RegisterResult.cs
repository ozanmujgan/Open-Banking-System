namespace VakifBankApi.Enums 
{
    public enum RegisterResult
    {
        Success,              // Kay�t i�lemi ba�ar�yla tamamland�
        UsernameAlreadyExists, // Belirtilen kullan�c� ad� zaten sistemde mevcut
        PasswordLengthInvalid  // �ifre belirlenen uzunluk kriterini kar��lam�yor
    }
}