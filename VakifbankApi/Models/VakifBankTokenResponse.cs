// VakifBankApi/Models/VakifBankTokenResponse.cs
using System.Text.Json.Serialization; 

namespace VakifBankApi.Models
{
	public class VakifBankTokenResponse
	{
		[System.Text.Json.Serialization.JsonPropertyName("access_token")] 
		public string AccessToken { get; set; } = string.Empty;

		[System.Text.Json.Serialization.JsonPropertyName("token_type")]
		public string TokenType { get; set; } = string.Empty;

		[System.Text.Json.Serialization.JsonPropertyName("expires_in")]
		public int ExpiresIn { get; set; } // Saniye cinsinden geçerlilik süresi

		[System.Text.Json.Serialization.JsonPropertyName("scope")] 
		public string Scope { get; set; } = string.Empty;
		// Eðer refresh_token veya baþka alanlar geliyorsa onlarý da buraya ekleyebilirsiniz
	}
}