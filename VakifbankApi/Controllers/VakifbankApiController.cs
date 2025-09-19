/*using Microsoft.AspNetCore.Mvc; // Controller için gerekli temel sýnýflar
using VakifBankApi.Services; // Servis katmanýndaki VakifBankApiService'i kullanmak için
using VakifBankApi.Models; // Modelleri (örneðin VakifBankAccountResponse) kullanmak için
using System;
using System.Threading.Tasks;

namespace VakifBankApi.Controllers
{
    [ApiController] // Bu sýnýfýn bir API Controller olduðunu belirtir
    [Route("api/[controller]")] // Bu controller'ýn kök yolunu belirler. "[controller]" yer tutucusu, sýnýf adýnýn "Controller" eki çýkarýlmýþ halini alýr (yani "VakifbankController" -> "Vakifbank"). Bu durumda URL "api/Vakifbank" olacaktýr.
    public class VakifbankController : ControllerBase // Tüm API Controller'larý ControllerBase'den türetilir
    {
        private readonly VakifBankApiService _vakifBankApiService; // VakifBankApiService servisini tutmak için özel alan

        // Yapýcý (Constructor): Dependency Injection ile VakifBankApiService servisini buraya alýyoruz.
        // ASP.NET Core, bu controller bir istek geldiðinde oluþturulduðunda VakifBankApiService'in bir örneðini otomatik olarak saðlar.
        public VakifbankController(VakifBankApiService vakifBankApiService)
        {
            _vakifBankApiService = vakifBankApiService;
        }

        // Endpoint: POST /api/Vakifbank/accounts
        // Bu metot, "/accounts" yoluna gelen POST isteklerini iþler.
        [HttpPost("accounts")]
        public async Task<IActionResult> GetAccounts()
        {
            try
            {
                // _vakifBankApiService üzerinden bankadan hesap listesini asenkron olarak alýyoruz.
                // Gelen yanýt, VakifBankAccountResponse modeline dönüþtürülmüþ haldedir.
                VakifBankAccountResponse? accounts = JsonSerializer.Deserialize<VakifBankAccountResponse>(responseBody);

                // Eðer servis yanýtý null dönerse veya içinde hesap verisi yoksa
                if (accounts == null || accounts.Data?.Accounts == null)
                {
                    // 500 Internal Server Error durum kodu ve açýklayýcý bir mesaj döndürüyoruz.
                    return StatusCode(500, new { message = "Hesap bilgileri alýnamadý: Yanýt boþ veya hesap verisi bulunamadý." });
                }

                // Baþarýlý durumda 200 OK durum kodu ve bankadan gelen modellenmiþ hesap verilerini JSON olarak döndürüyoruz.
                return Ok(accounts);
            }
            catch (Exception ex) // Servis katmanýndan veya baþka bir yerden gelen herhangi bir hatayý yakalarýz
            {
                // Hatayý konsola loglarýz.
                Console.WriteLine($"API Controller Hatasý: Hesap bilgileri alýnýrken hata oluþtu: {ex.Message}");
                // Ýstemciye 500 Internal Server Error durum kodu ve hata mesajýný içeren bir yanýt döndürüyoruz.
                return StatusCode(500, new { message = $"Hesap bilgileri alýnamadý: {ex.Message}" });
            }
        }
    }
}*/