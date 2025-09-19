namespace VakifBankApi.Models
{
    public class VakifBankApiSettings
    {
        public string TokenUrl { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string client_id { get; set; } = string.Empty; 
        public string client_secret { get; set; } = string.Empty;
        public string consentId { get; set; } = string.Empty; 
        public string scope { get; set; } = string.Empty;
        public string resource { get; set; } = string.Empty;
    }
}