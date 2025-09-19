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
		public int ExpiresIn { get; set; } // Saniye cinsinden ge�erlilik s�resi

		[System.Text.Json.Serialization.JsonPropertyName("scope")] 
		public string Scope { get; set; } = string.Empty;
		// E�er refresh_token veya ba�ka alanlar geliyorsa onlar� da buraya ekleyebilirsiniz
	}
}