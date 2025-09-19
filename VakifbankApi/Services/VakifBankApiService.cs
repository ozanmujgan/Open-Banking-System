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
                _logger.LogInformation("Vak�fBank token al�m� ba�lat�l�yor veya yenileniyor.");
                _logger.LogDebug("Token yenileme kriterleri: Token bo� mu? {IsTokenEmpty}, S�re dolmak �zere mi? {IsTokenExpiring}", string.IsNullOrEmpty(_accessToken), _tokenExpiration <= DateTime.UtcNow.AddMinutes(5));

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
                    _logger.LogInformation($"Token iste�i g�nderilecek URL: {_settings.TokenUrl}");
                    _logger.LogDebug("Token iste�i i�eri�i: Client ID: {ClientId}, Scope: {Scope}", _settings.client_id, _settings.scope);

                    response = await _httpClient.PostAsync(_settings.TokenUrl, content);

                    // Ba�ar�l� durum kodunda de�ilse, exception f�rlatmadan �nce yan�t� oku
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorResponseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Vak�fBank Token iste�i HTTP hata kodu d�nd�rd�: {StatusCode}. Yan�t i�eri�i: {ErrorContent}", response.StatusCode, errorResponseContent);
                        response.EnsureSuccessStatusCode(); // Yine de exception f�rlatmak i�in
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Vak�fBank Token yan�t� (RAW): {JsonResponse}", jsonResponse); // Ham yan�t� logla

                    var tokenResponse = JsonSerializer.Deserialize<VakifBankTokenResponse>(jsonResponse);

                    if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken))
                    {
                        _accessToken = tokenResponse.AccessToken;
                        _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                        _logger.LogInformation("Vak�fBank Access Token ba�ar�yla al�nd� ve kaydedildi. Token S�resi (saniye): {ExpiresIn}", tokenResponse.ExpiresIn);
                        return _accessToken;
                    }
                    else
                    {
                        _logger.LogError("Vak�fBank'tan Access Token al�namad� veya bo� d�nd�. Yan�t: {JsonResponse}", jsonResponse);
                        throw new Exception("Vak�fBank'tan Access Token al�namad� veya bo� d�nd�.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Vak�fBank Token iste�i s�ras�nda HTTP hatas� olu�tu: {Message}. HTTP Durum Kodu: {StatusCode}", ex.Message, ex.StatusCode);
                    // Daha �nce logland�, ancak bu blok da yakalar.
                    throw new Exception($"Vak�fBank Token hatas�: {ex.Message}", ex);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Vak�fBank Token yan�t� JSON ��z�mleme hatas�: {Message}. Yanl�� JSON format� olabilir.", ex.Message);
                    throw new Exception($"VakifBank Token yan�t� JSON hatas�: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GetAccessTokenAsync metodunda beklenmeyen bir hata olu�tu: {Message}", ex.Message);
                    throw;
                }
            }
            _logger.LogInformation("Mevcut Vak�fBank Access Token kullan�lacak. S�resi: {TokenExpiration}", _tokenExpiration);
            return _accessToken!;
        }

        public async Task<VakifBankAccountResponse?> GetAccountsAsync()
        {
            var token = await GetAccessTokenAsync(); // Bankadan token'� al
            _logger.LogDebug("GetAccountsAsync i�in al�nan Banka API token'�: {Token}", token);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var requestBody = new object(); // Bo� JSON objesi "{}"
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;

            try
            {
                _logger.LogInformation("Vak�fBank Hesap Listesi iste�i g�nderiliyor. URL: {BaseAddress}accountList, Body: {Body}", _httpClient.BaseAddress, jsonContent);
                response = await _httpClient.PostAsync("accountList", content);

                // Bankadan gelen HTTP yan�t�n�n tamam�n� logla
                _logger.LogInformation("Vak�fBank Hesap Listesi iste�i yan�t� - HTTP Durum Kodu: {StatusCode}", response.StatusCode);

                string jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Vak�fBank Hesap Listesi yan�t� (RAW): {JsonResponse}", jsonResponse); // Ham yan�t� logla

                // Ba�ar�s�z durum kodunda dahi yan�t� okuduktan sonra kontrol et
                response.EnsureSuccessStatusCode(); // 2xx olmayan durum kodlar�nda HttpRequestException f�rlat�r

                var bankResponse = JsonSerializer.Deserialize<VakifBankAccountResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (bankResponse == null)
                {
                    _logger.LogError("Vak�fBank Hesap Listesi yan�t� bo� veya ge�ersiz (Deserialization sonras�). JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("Vak�fBank Hesap Listesi yan�t� bo� veya ge�ersiz.");
                }

                // Bankan�n kendi Header StatusCode kontrol�
                if (bankResponse.Header == null)
                {
                    _logger.LogError("Vak�fBank Hesap Listesi yan�t�nda Header objesi bulunamad�. JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("Vak�fBank Hesap Listesi yan�t�nda Header bilgisi eksik.");
                }
                if (bankResponse.Header.StatusCode != "00000")
                {
                    _logger.LogError("Vak�fBank Hesap Listesi yan�t� hata kodu d�nd�rd�. StatusCode: {StatusCode}, StatusDescription: {StatusDescription}", bankResponse.Header.StatusCode, bankResponse.Header.StatusDescription);
                    // Burada Problem Details d�nd�r�ld��� i�in ayr� bir Exception f�rlatmayabiliriz, Program.cs zaten bunu handle edecek
                    // Ancak loglamak �nemli.
                }

                _logger.LogInformation("Vak�fBank Hesap Listesi ba�ar�yla al�nd�. Banka API Status Kodu: {BankStatusCode}. Toplam {Count} hesap bulundu.", bankResponse.Header?.StatusCode, bankResponse.Data?.Accounts?.Count ?? 0);
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
                _logger.LogError(ex, "Vak�fBank Hesap bilgisi iste�i s�ras�nda HTTP hatas� olu�tu: {Message}. HTTP Durum Kodu: {StatusCode}", ex.Message, ex.StatusCode);
                // Yan�t i�eri�i daha �nce logland�, burada sadece genel exception'� tekrar f�rlat�yoruz.
                throw new Exception($"Vak�fBank Hesap hatas�: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Vak�fBank Hesap yan�t� JSON ��z�mleme hatas�: {Message}. Ham yan�t yanl�� formatta olabilir. Yan�t okunamad�.", ex.Message);
                throw new Exception($"VakifBank Hesap yan�t� JSON hatas�: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAccountsAsync metodunda beklenmeyen genel hata olu�tu: {Message}", ex.Message);
                throw;
            }
        }
        // Hesap Detay� Metodu: GetAccountDetailAsync
        public async Task<VakifBankAccountDetailResponse?> GetAccountDetailAsync(VakifBankAccountDetailRequest request)
        {
            var token = await GetAccessTokenAsync(); // Bankadan token'� al
            _logger.LogDebug("GetAccountDetailAsync i�in al�nan Banka API token'�: {Token}", token);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // Token'� Authorization header'�na ekle

            var jsonContent = JsonSerializer.Serialize(request); // �ste�i JSON'a �evir
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"); // JSON i�eri�i olu�tur

            HttpResponseMessage? response = null;

            try
            {
                _logger.LogInformation("Vak�fBank Hesap Detay� iste�i g�nderiliyor. URL: {BaseAddress}accountDetail, Body: {Body}", _httpClient.BaseAddress, jsonContent);
                // "/accountDetail" endpoint'ine POST iste�i g�nder. BaseAddress ile birle�ecek.
                response = await _httpClient.PostAsync("accountDetail", content);

                _logger.LogInformation("Vak�fBank Hesap Detay� iste�i yan�t� - HTTP Durum Kodu: {StatusCode}", response.StatusCode);

                string jsonResponse = await response.Content.ReadAsStringAsync(); // Yan�t i�eri�ini string olarak oku
                _logger.LogDebug("Vak�fBank Hesap Detay� yan�t� (RAW): {JsonResponse}", jsonResponse); // Ham yan�t� logla

                response.EnsureSuccessStatusCode(); // 2xx olmayan durum kodlar�nda HttpRequestException f�rlat�r

                // Yan�t� VakifBankAccountDetailResponse modeline d�n��t�r (case-insensitive)
                var bankResponse = JsonSerializer.Deserialize<VakifBankAccountDetailResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (bankResponse == null)
                {
                    _logger.LogError("Vak�fBank Hesap Detay� yan�t� bo� veya ge�ersiz (Deserialization sonras�). JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("Vak�fBank Hesap Detay� yan�t� bo� veya ge�ersiz.");
                }

                // Bankan�n kendi Header StatusCode kontrol�
                if (bankResponse.Header == null)
                {
                    _logger.LogError("Vak�fBank Hesap Detay� yan�t�nda Header objesi bulunamad�. JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("Vak�fBank Hesap Detay� yan�t�nda Header bilgisi eksik.");
                }
                // Ba�ar�l� kod "APIGW000000" olarak bekleniyor (�nceki API'lerle ayn� varsay�m� yap�yoruz)
                if (bankResponse.Header.StatusCode != "APIGW000000")
                {
                    _logger.LogError("Vak�fBank Hesap Detay� yan�t� hata kodu d�nd�rd�. StatusCode: {StatusCode}, StatusDescription: {StatusDescription}", bankResponse.Header.StatusCode, bankResponse.Header.StatusDescription);
                    // Burada duruma g�re �zel bir Problem Details f�rlat�labilir veya d�n�lebilir.
                }

                _logger.LogInformation("Vak�fBank Hesap Detay� ba�ar�yla al�nd�. Banka API Status Kodu: {BankStatusCode}. Hesap Numaras�: {AccountNumber}", bankResponse.Header?.StatusCode, bankResponse.Data?.AccountInfo?.AccountNumber);
                // Yan�ttaki AccountInfo objesi null de�ilse detaylar� logla
                if (bankResponse.Data?.AccountInfo != null)
                {
                    var accountInfo = bankResponse.Data.AccountInfo;
                    _logger.LogDebug("- IBAN: {IBAN}, Bakiye: {Balance} {CurrencyCode}, Hesap Durumu: {AccountStatus}, M��teri No: {CustomerNumber}, �ube Kodu: {BranchCode}",
                                     accountInfo.IBAN, accountInfo.Balance, accountInfo.CurrencyCode, accountInfo.AccountStatus, accountInfo.CustomerNumber, accountInfo.BranchCode);
                    // Daha fazla detay loglamak isterseniz buraya ekleyebilirsiniz.
                }
                return bankResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Vak�fBank Hesap Detay� iste�i s�ras�nda HTTP hatas� olu�tu: {Message}. HTTP Durum Kodu: {StatusCode}", ex.Message, ex.StatusCode);
                throw new Exception($"Vak�fBank Hesap Detay� hatas�: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Vak�fBank Hesap Detay� yan�t� JSON ��z�mleme hatas�: {Message}. Ham yan�t yanl�� formatta olabilir. Yan�t okunamad�.", ex.Message);
                throw new Exception($"VakifBank Hesap Detay� yan�t� JSON hatas�: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAccountDetailAsync metodunda beklenmeyen genel hata olu�tu: {Message}", ex.Message);
                throw;
            }
        }
        // Yeni metod: Hesap Hareketlerini �ekme
        public async Task<VakifBankAccountTransactionsResponse?> GetAccountTransactionsAsync(VakifBankAccountTransactionsRequest request)
        {
            var token = await GetAccessTokenAsync(); // Bankadan token'� al
            _logger.LogDebug("GetAccountTransactionsAsync i�in al�nan Banka API token'�: {Token}", token);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonContent = JsonSerializer.Serialize(request); // �ste�i JSON'a �evir
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;

            try
            {
                _logger.LogInformation("Vak�fBank Hesap Hareketleri iste�i g�nderiliyor. URL: {BaseAddress}accountTransactions, Body: {Body}", _httpClient.BaseAddress, jsonContent);
                // Dok�mantasyonda path "/accountTransactions" olarak belirtilmi�
                response = await _httpClient.PostAsync("accountTransactions", content);

                _logger.LogInformation("Vak�fBank Hesap Hareketleri iste�i yan�t� - HTTP Durum Kodu: {StatusCode}", response.StatusCode);

                string jsonResponse = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Vak�fBank Hesap Hareketleri yan�t� (RAW): {JsonResponse}", jsonResponse); // Ham yan�t� logla

                response.EnsureSuccessStatusCode(); // 2xx olmayan durum kodlar�nda HttpRequestException f�rlat�r

                var bankResponse = JsonSerializer.Deserialize<VakifBankAccountTransactionsResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (bankResponse == null)
                {
                    _logger.LogError("Vak�fBank Hesap Hareketleri yan�t� bo� veya ge�ersiz (Deserialization sonras�). JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("Vak�fBank Hesap Hareketleri yan�t� bo� veya ge�ersiz.");
                }

                // Bankan�n kendi Header StatusCode kontrol�
                if (bankResponse.Header == null)
                {
                    _logger.LogError("Vak�fBank Hesap Hareketleri yan�t�nda Header objesi bulunamad�. JSON: {JsonResponse}", jsonResponse);
                    throw new Exception("Vak�fBank Hesap Hareketleri yan�t�nda Header bilgisi eksik.");
                }
                // Ba�ar�l� kod "APIGW000000" olarak bekleniyor
                if (bankResponse.Header.StatusCode != "APIGW000000")
                {
                    _logger.LogError("Vak�fBank Hesap Hareketleri yan�t� hata kodu d�nd�rd�. StatusCode: {StatusCode}, StatusDescription: {StatusDescription}", bankResponse.Header.StatusCode, bankResponse.Header.StatusDescription);
                    // Burada Problem Details d�nd�r�lecek
                }

                _logger.LogInformation("Vak�fBank Hesap Hareketleri ba�ar�yla al�nd�. Banka API Status Kodu: {BankStatusCode}. Toplam {Count} hareket bulundu.", bankResponse.Header?.StatusCode, bankResponse.Data?.AccountTransactions?.Count ?? 0);
                if (bankResponse.Data?.AccountTransactions != null)
                {
                    foreach (var transaction in bankResponse.Data.AccountTransactions)
                    {
                        _logger.LogDebug("- ��lem Tarihi: {TransactionDate}, Tutar: {Amount} {CurrencyCode}, A��klama: {Description}", transaction.TransactionDate, transaction.Amount, transaction.CurrencyCode, transaction.Description);
                    }
                }
                return bankResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Vak�fBank Hesap Hareketleri iste�i s�ras�nda HTTP hatas� olu�tu: {Message}. HTTP Durum Kodu: {StatusCode}", ex.Message, ex.StatusCode);
                throw new Exception($"Vak�fBank Hesap Hareketleri hatas�: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Vak�fBank Hesap Hareketleri yan�t� JSON ��z�mleme hatas�: {Message}. Ham yan�t yanl�� formatta olabilir. Yan�t okunamad�.", ex.Message);
                throw new Exception($"VakifBank Hesap Hareketleri yan�t� JSON hatas�: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAccountTransactionsAsync metodunda beklenmeyen genel hata olu�tu: {Message}", ex.Message);
                throw;
            }
        }
    }
}