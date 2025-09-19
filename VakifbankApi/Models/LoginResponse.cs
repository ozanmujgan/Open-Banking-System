namespace VakifBankApi.Models 
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; } // Kendi veritabaný kullanýcý ID'miz
        public string Username { get; set; } = string.Empty;
    }
}