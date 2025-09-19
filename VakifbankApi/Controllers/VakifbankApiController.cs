/*using Microsoft.AspNetCore.Mvc; // Controller i�in gerekli temel s�n�flar
using VakifBankApi.Services; // Servis katman�ndaki VakifBankApiService'i kullanmak i�in
using VakifBankApi.Models; // Modelleri (�rne�in VakifBankAccountResponse) kullanmak i�in
using System;
using System.Threading.Tasks;

namespace VakifBankApi.Controllers
{
    [ApiController] // Bu s�n�f�n bir API Controller oldu�unu belirtir
    [Route("api/[controller]")] // Bu controller'�n k�k yolunu belirler. "[controller]" yer tutucusu, s�n�f ad�n�n "Controller" eki ��kar�lm�� halini al�r (yani "VakifbankController" -> "Vakifbank"). Bu durumda URL "api/Vakifbank" olacakt�r.
    public class VakifbankController : ControllerBase // T�m API Controller'lar� ControllerBase'den t�retilir
    {
        private readonly VakifBankApiService _vakifBankApiService; // VakifBankApiService servisini tutmak i�in �zel alan

        // Yap�c� (Constructor): Dependency Injection ile VakifBankApiService servisini buraya al�yoruz.
        // ASP.NET Core, bu controller bir istek geldi�inde olu�turuldu�unda VakifBankApiService'in bir �rne�ini otomatik olarak sa�lar.
        public VakifbankController(VakifBankApiService vakifBankApiService)
        {
            _vakifBankApiService = vakifBankApiService;
        }

        // Endpoint: POST /api/Vakifbank/accounts
        // Bu metot, "/accounts" yoluna gelen POST isteklerini i�ler.
        [HttpPost("accounts")]
        public async Task<IActionResult> GetAccounts()
        {
            try
            {
                // _vakifBankApiService �zerinden bankadan hesap listesini asenkron olarak al�yoruz.
                // Gelen yan�t, VakifBankAccountResponse modeline d�n��t�r�lm�� haldedir.
                VakifBankAccountResponse? accounts = JsonSerializer.Deserialize<VakifBankAccountResponse>(responseBody);

                // E�er servis yan�t� null d�nerse veya i�inde hesap verisi yoksa
                if (accounts == null || accounts.Data?.Accounts == null)
                {
                    // 500 Internal Server Error durum kodu ve a��klay�c� bir mesaj d�nd�r�yoruz.
                    return StatusCode(500, new { message = "Hesap bilgileri al�namad�: Yan�t bo� veya hesap verisi bulunamad�." });
                }

                // Ba�ar�l� durumda 200 OK durum kodu ve bankadan gelen modellenmi� hesap verilerini JSON olarak d�nd�r�yoruz.
                return Ok(accounts);
            }
            catch (Exception ex) // Servis katman�ndan veya ba�ka bir yerden gelen herhangi bir hatay� yakalar�z
            {
                // Hatay� konsola loglar�z.
                Console.WriteLine($"API Controller Hatas�: Hesap bilgileri al�n�rken hata olu�tu: {ex.Message}");
                // �stemciye 500 Internal Server Error durum kodu ve hata mesaj�n� i�eren bir yan�t d�nd�r�yoruz.
                return StatusCode(500, new { message = $"Hesap bilgileri al�namad�: {ex.Message}" });
            }
        }
    }
}*/