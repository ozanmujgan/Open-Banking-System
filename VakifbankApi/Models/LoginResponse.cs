namespace VakifBankApi.Models 
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; } // Kendi veritaban� kullan�c� ID'miz
        public string Username { get; set; } = string.Empty;
    }
}