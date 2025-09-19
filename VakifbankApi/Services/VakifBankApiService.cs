using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using VakifBankApi.Models;
using System.Text;
using System;
using Microsoft.Extensions.Logging;

namespace VakifBankApi.Services
{
    public class VakifBankApiService
    {
        private readonly HttpClient _httpClient;
        private readonly VakifBankApiSettings _settings;
        private string? _accessToken;
        private DateTime _tokenExpiration;
        private readonly ILogger<VakifBankApiService> _logger;

        public VakifBankApiService(HttpClient httpClient, IOptions<VakifBankApiSettings> settings, ILogger<VakifBankApiService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl!);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (string.IsNullOrEmpty(_accessToken) || _tokenExpiration <= DateTime.UtcNow.AddMinutes(5))
            {
                _logger.LogInformation("VakýfBank token alýmý baþlatýlýyor veya yenileniyor.");
                _logger.LogDebug("Token yenileme kriterleri: Token boþ mu? {IsTokenEmpty}, Süre dolmak üzere mi? {IsTokenExpiring}", string.IsNullOrEmpty(_accessToken), _tokenExpiration <= DateTime.UtcNow.AddMinutes(5));

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", _settings.client_id),
                    new KeyValuePair<string, string>("client_secret", _settings.client_secret),
                    new KeyValuePair<string, string>("grant_type", "b2b_credentials"),
                    new KeyValuePair<string, string>("scope", _settings.scope),
                    new KeyValuePair<string, string>("consentId", _settings.consentId),
                    new KeyValuePair<string, string>("resource", _settings.resource)
                });

                HttpResponseMessage? response = null;

                try
                {
                    _logger.LogInformation($"Token isteði gönderilecek URL: {_settings.TokenUrl}");
                    _logger.LogDebug("Token isteði içeriði: Client ID: {ClientId}, Scope: {Scope}", _settings.client_id, _settings.scope);

                    response = await _httpClient.PostAsync(_settings.TokenUrl, content);

                    // Baþarýlý durum kodunda deðilse, exception fýrlatmadan önce yanýtý oku
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("VakýfBank Token isteði HTTP hata kodu döndürdü: {StatusCode}. Yanýt içeriði: {ErrorContent}", response.StatusCode, errorResponseContent);
                        response.EnsureSuccessStatusCode(); // Yine de exception fýrlatmak için
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("VakýfBank Token yanýtý (RAW): {JsonResponse}", jsonResponse); // Ham yanýtý logla

                    var tokenResponse = JsonSerializer.Deserialize<VakifBankTokenResponse>(jsonResponse);

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                    {
                        _accessToken = tokenResponse.AccessToken;
                        _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                        _logger.LogInformation("VakýfBank Access Token baþarýyla alýndý ve kaydedildi. Token Süresi (saniye): {ExpiresIn}", tokenResponse.ExpiresIn);
                        return _accessToken;
                    }
                    else
                    {
                        _logger.LogError("VakýfBank'tan Access Token alýnamadý veya boþ döndü. Yanýt: {JsonResponse}", jsonResponse);
                        throw new Exception("VakýfBank'tan Access Token alýnamadý veya boþ döndü.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "VakýfBank Token isteði sýrasýnda HTTP hatasý oluþtu: {Message}. HTTP Durum Kodu: {StatusCode}", ex.Message, ex.StatusCode);
                    // Daha önce loglandý, ancak bu blok da yakalar.
                    throw new Exception($"VakýfBank Token hatasý: {ex.Message}", ex);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "VakýfBank Token yanýtý JSON çözümleme hatasý: {Message}. Yanlýþ JSON formatý olabilir.", ex.Message);
                    throw new Exception($"VakifBank Token yanýtý JSON hatasý: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GetAccessTokenAsync metodunda beklenmeyen bir hata oluþtu: {Message}", ex.Message);
                    throw;
                }
            }
            _logger.LogInformation("Mevcut VakýfBank Access Token kullanýlacak. Süresi: {TokenExpiration}", _tokenExpiration);
            return _accessToken!;
        }

        public async Task<VakifBankAccountResponse?> GetAccountsAsync()
        {
            var token = await GetAccessTokenAsync(); // Bankadan token'ý al
            _logger.LogDebug("GetAccountsAsync için alýnan Banka API token'ý: {Token}", token);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestBody = new object(); // Boþ JSON objesi "{}"
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;

            try
            {
                _logger.LogInformation("VakýfBank Hesap Listesi isteði gönderiliyor. URL: {BaseAddress}accountList, Body: {Body}", _httpClient.BaseAddress, jsonContent);
                response = await _httpClient.PostAsync("accountList", content);

                // Bankadan gelen HTTP yanýtýnýn tamamýný logla
                _logger.LogInformation("VakýfBank Hesap Listesi isteði yanýtý - HTTP Durum Kodu: {StatusCode}", response.StatusCode);

                string jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("VakýfBank Hesap Listesi yanýtý (RAW): {JsonResponse}", jsonResponse); // Ham yanýtý logla

                // Baþarýsýz durum kodunda dahi yanýtý okuduktan sonra kontrol et
                response.EnsureSuccessStatusCode(); // 2xx olmayan durum kodlarýnda HttpRequestException fýrlatýr

                var bankResponse = JsonSerializer.Deserialize<VakifBankAccountResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (bankResponse == null)
                {
                    _logger.LogError("VakýfBank Hesap Listesi yanýtý boþ veya geçersiz (Deserialization sonrasý). JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("VakýfBank Hesap Listesi yanýtý boþ veya geçersiz.");
                }

                // Bankanýn kendi Header StatusCode kontrolü
                if (bankResponse.Header == null)
                {
                    _logger.LogError("VakýfBank Hesap Listesi yanýtýnda Header objesi bulunamadý. JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("VakýfBank Hesap Listesi yanýtýnda Header bilgisi eksik.");
                }
                if (bankResponse.Header.StatusCode != "00000")
                {
                    _logger.LogError("VakýfBank Hesap Listesi yanýtý hata kodu döndürdü. StatusCode: {StatusCode}, StatusDescription: {StatusDescription}", bankResponse.Header.StatusCode, bankResponse.Header.StatusDescription);
                    // Burada Problem Details döndürüldüðü için ayrý bir Exception fýrlatmayabiliriz, Program.cs zaten bunu handle edecek
                    // Ancak loglamak önemli.
                }

                _logger.LogInformation("VakýfBank Hesap Listesi baþarýyla alýndý. Banka API Status Kodu: {BankStatusCode}. Toplam {Count} hesap bulundu.", bankResponse.Header?.StatusCode, bankResponse.Data?.Accounts?.Count ?? 0);
                if (bankResponse.Data?.Accounts != null)
                {
                    foreach (var account in bankResponse.Data.Accounts)
                    {
                        _logger.LogDebug("- IBAN: {IBAN}, Bakiye: {Balance} {CurrencyCode}", account.IBAN, account.Balance, account.CurrencyCode);
                    }
                }
                return bankResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "VakýfBank Hesap bilgisi isteði sýrasýnda HTTP hatasý oluþtu: {Message}. HTTP Durum Kodu: {StatusCode}", ex.Message, ex.StatusCode);
                // Yanýt içeriði daha önce loglandý, burada sadece genel exception'ý tekrar fýrlatýyoruz.
                throw new Exception($"VakýfBank Hesap hatasý: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "VakýfBank Hesap yanýtý JSON çözümleme hatasý: {Message}. Ham yanýt yanlýþ formatta olabilir. Yanýt okunamadý.", ex.Message);
                throw new Exception($"VakifBank Hesap yanýtý JSON hatasý: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAccountsAsync metodunda beklenmeyen genel hata oluþtu: {Message}", ex.Message);
                throw;
            }
        }
        // Hesap Detayý Metodu: GetAccountDetailAsync
        public async Task<VakifBankAccountDetailResponse?> GetAccountDetailAsync(VakifBankAccountDetailRequest request)
        {
            var token = await GetAccessTokenAsync(); // Bankadan token'ý al
            _logger.LogDebug("GetAccountDetailAsync için alýnan Banka API token'ý: {Token}", token);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // Token'ý Authorization header'ýna ekle

            var jsonContent = JsonSerializer.Serialize(request); // Ýsteði JSON'a çevir
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"); // JSON içeriði oluþtur

            HttpResponseMessage? response = null;

            try
            {
                _logger.LogInformation("VakýfBank Hesap Detayý isteði gönderiliyor. URL: {BaseAddress}accountDetail, Body: {Body}", _httpClient.BaseAddress, jsonContent);
                // "/accountDetail" endpoint'ine POST isteði gönder. BaseAddress ile birleþecek.
                response = await _httpClient.PostAsync("accountDetail", content);

                _logger.LogInformation("VakýfBank Hesap Detayý isteði yanýtý - HTTP Durum Kodu: {StatusCode}", response.StatusCode);

                string jsonResponse = await response.Content.ReadAsStringAsync(); // Yanýt içeriðini string olarak oku
                _logger.LogDebug("VakýfBank Hesap Detayý yanýtý (RAW): {JsonResponse}", jsonResponse); // Ham yanýtý logla

                response.EnsureSuccessStatusCode(); // 2xx olmayan durum kodlarýnda HttpRequestException fýrlatýr

                // Yanýtý VakifBankAccountDetailResponse modeline dönüþtür (case-insensitive)
                var bankResponse = JsonSerializer.Deserialize<VakifBankAccountDetailResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (bankResponse == null)
                {
                    _logger.LogError("VakýfBank Hesap Detayý yanýtý boþ veya geçersiz (Deserialization sonrasý). JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("VakýfBank Hesap Detayý yanýtý boþ veya geçersiz.");
                }

                // Bankanýn kendi Header StatusCode kontrolü
                if (bankResponse.Header == null)
                {
                    _logger.LogError("VakýfBank Hesap Detayý yanýtýnda Header objesi bulunamadý. JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("VakýfBank Hesap Detayý yanýtýnda Header bilgisi eksik.");
                }
                // Baþarýlý kod "APIGW000000" olarak bekleniyor (önceki API'lerle ayný varsayýmý yapýyoruz)
                if (bankResponse.Header.StatusCode != "APIGW000000")
                {
                    _logger.LogError("VakýfBank Hesap Detayý yanýtý hata kodu döndürdü. StatusCode: {StatusCode}, StatusDescription: {StatusDescription}", bankResponse.Header.StatusCode, bankResponse.Header.StatusDescription);
                    // Burada duruma göre özel bir Problem Details fýrlatýlabilir veya dönülebilir.
                }

                _logger.LogInformation("VakýfBank Hesap Detayý baþarýyla alýndý. Banka API Status Kodu: {BankStatusCode}. Hesap Numarasý: {AccountNumber}", bankResponse.Header?.StatusCode, bankResponse.Data?.AccountInfo?.AccountNumber);
                // Yanýttaki AccountInfo objesi null deðilse detaylarý logla
                if (bankResponse.Data?.AccountInfo != null)
                {
                    var accountInfo = bankResponse.Data.AccountInfo;
                    _logger.LogDebug("- IBAN: {IBAN}, Bakiye: {Balance} {CurrencyCode}, Hesap Durumu: {AccountStatus}, Müþteri No: {CustomerNumber}, Þube Kodu: {BranchCode}",
                                     accountInfo.IBAN, accountInfo.Balance, accountInfo.CurrencyCode, accountInfo.AccountStatus, accountInfo.CustomerNumber, accountInfo.BranchCode);
                    // Daha fazla detay loglamak isterseniz buraya ekleyebilirsiniz.
                }
                return bankResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "VakýfBank Hesap Detayý isteði sýrasýnda HTTP hatasý oluþtu: {Message}. HTTP Durum Kodu: {StatusCode}", ex.Message, ex.StatusCode);
                throw new Exception($"VakýfBank Hesap Detayý hatasý: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "VakýfBank Hesap Detayý yanýtý JSON çözümleme hatasý: {Message}. Ham yanýt yanlýþ formatta olabilir. Yanýt okunamadý.", ex.Message);
                throw new Exception($"VakifBank Hesap Detayý yanýtý JSON hatasý: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAccountDetailAsync metodunda beklenmeyen genel hata oluþtu: {Message}", ex.Message);
                throw;
            }
        }
        // Yeni metod: Hesap Hareketlerini Çekme
        public async Task<VakifBankAccountTransactionsResponse?> GetAccountTransactionsAsync(VakifBankAccountTransactionsRequest request)
        {
            var token = await GetAccessTokenAsync(); // Bankadan token'ý al
            _logger.LogDebug("GetAccountTransactionsAsync için alýnan Banka API token'ý: {Token}", token);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonContent = JsonSerializer.Serialize(request); // Ýsteði JSON'a çevir
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;

            try
            {
                _logger.LogInformation("VakýfBank Hesap Hareketleri isteði gönderiliyor. URL: {BaseAddress}accountTransactions, Body: {Body}", _httpClient.BaseAddress, jsonContent);
                // Dokümantasyonda path "/accountTransactions" olarak belirtilmiþ
                response = await _httpClient.PostAsync("accountTransactions", content);

                _logger.LogInformation("VakýfBank Hesap Hareketleri isteði yanýtý - HTTP Durum Kodu: {StatusCode}", response.StatusCode);

                string jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("VakýfBank Hesap Hareketleri yanýtý (RAW): {JsonResponse}", jsonResponse); // Ham yanýtý logla

                response.EnsureSuccessStatusCode(); // 2xx olmayan durum kodlarýnda HttpRequestException fýrlatýr

                var bankResponse = JsonSerializer.Deserialize<VakifBankAccountTransactionsResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (bankResponse == null)
                {
                    _logger.LogError("VakýfBank Hesap Hareketleri yanýtý boþ veya geçersiz (Deserialization sonrasý). JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("VakýfBank Hesap Hareketleri yanýtý boþ veya geçersiz.");
                }

                // Bankanýn kendi Header StatusCode kontrolü
                if (bankResponse.Header == null)
                {
                    _logger.LogError("VakýfBank Hesap Hareketleri yanýtýnda Header objesi bulunamadý. JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("VakýfBank Hesap Hareketleri yanýtýnda Header bilgisi eksik.");
                }
                // Baþarýlý kod "APIGW000000" olarak bekleniyor
                if (bankResponse.Header.StatusCode != "APIGW000000")
                {
                    _logger.LogError("VakýfBank Hesap Hareketleri yanýtý hata kodu döndürdü. StatusCode: {StatusCode}, StatusDescription: {StatusDescription}", bankResponse.Header.StatusCode, bankResponse.Header.StatusDescription);
                    // Burada Problem Details döndürülecek
                }

                _logger.LogInformation("VakýfBank Hesap Hareketleri baþarýyla alýndý. Banka API Status Kodu: {BankStatusCode}. Toplam {Count} hareket bulundu.", bankResponse.Header?.StatusCode, bankResponse.Data?.AccountTransactions?.Count ?? 0);
                if (bankResponse.Data?.AccountTransactions != null)
                {
                    foreach (var transaction in bankResponse.Data.AccountTransactions)
                    {
                        _logger.LogDebug("- Ýþlem Tarihi: {TransactionDate}, Tutar: {Amount} {CurrencyCode}, Açýklama: {Description}", transaction.TransactionDate, transaction.Amount, transaction.CurrencyCode, transaction.Description);
                    }
                }
                return bankResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "VakýfBank Hesap Hareketleri isteði sýrasýnda HTTP hatasý oluþtu: {Message}. HTTP Durum Kodu: {StatusCode}", ex.Message, ex.StatusCode);
                throw new Exception($"VakýfBank Hesap Hareketleri hatasý: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "VakýfBank Hesap Hareketleri yanýtý JSON çözümleme hatasý: {Message}. Ham yanýt yanlýþ formatta olabilir. Yanýt okunamadý.", ex.Message);
                throw new Exception($"VakifBank Hesap Hareketleri yanýtý JSON hatasý: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAccountTransactionsAsync metodunda beklenmeyen genel hata oluþtu: {Message}", ex.Message);
                throw;
            }
        }
    }
}