namespace VakifBankApi.Models 
{
    public class User
    {
        public int Id { get; set; } // Primary Key
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // G�venli �ifre saklama i�in hash
    }
}