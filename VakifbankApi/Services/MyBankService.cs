using Microsoft.EntityFrameworkCore;
using VakifBankApi.Data; // Kendi DbContext'imizi kullanabilmek için
using VakifBankApi.Models; // Modellerimizi kullanabilmek için
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using VakifBankApi.Enums;
namespace VakifBankApi.Services 
{
    // Bu sýnýf, kendi veritabanýmýzdaki (VakifBankDb) iþlemlerden sorumlu olacak
    public class MyBankService
    {
        private readonly VakifBankDbContext _context; // Kendi DbContext'imizi kullanýyoruz
        private readonly ILogger<MyBankService> _logger;

        public MyBankService(VakifBankDbContext context, ILogger<MyBankService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> ValidateUser(string username, string password) // Metot adýný ValidateUser olarak deðiþtirdim
        {
            _logger.LogInformation("Kullanýcý doðrulama denemesi: Kullanýcý adý - {Username}", username);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                _logger.LogWarning("Kullanýcý doðrulama baþarýsýz: Kullanýcý '{Username}' bulunamadý.", username);
                return null;
            }

            var hashedPassword = HashPassword(password);

            if (hashedPassword != user.PasswordHash)
            {
                _logger.LogWarning("Kullanýcý doðrulama baþarýsýz: '{Username}' için geçersiz þifre.", username);
                return null;
            }

            _logger.LogInformation("Kullanýcý doðrulama baþarýlý: Kullanýcý '{Username}' doðrulandý.", username);
            return user;
        }

        // Þifre Hash'leme Yardýmcý Metodu
        public string HashPassword(string password) // Public yaptým, istersen Program.cs'de de çaðýrabiliriz
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // Hesap Listesi Metodu
        public async Task<List<BankAccount>> GetBankAccountsByUserId(int userId)
        {
            _logger.LogInformation("Kullanýcý ID {UserId} için banka hesaplarý getiriliyor.", userId);
            return await _context.BankAccounts
                                 .Where(a => a.UserId == userId)
                                 .ToListAsync();
        }

        // Hesap Detay Metodu (IBAN'a göre, kullanýcýnýn kendi hesabý olduðunu kontrol etmeli)
        public async Task<BankAccount?> GetAccountDetailByIBAN(string iban, int userId)
        {
            _logger.LogInformation("IBAN {IBAN} için hesap detayý getiriliyor, kullanýcý ID: {UserId}.", iban, userId);
            return await _context.BankAccounts
                                 .FirstOrDefaultAsync(a => a.IBAN == iban && a.UserId == userId);
        }

        // Ýþlem Listesi Metodu (Hesap ID'sine göre, kullanýcýnýn kendi hesabý olduðunu kontrol etmeli)
        public async Task<List<BankTransaction>> GetTransactionsByAccountId(
    int bankAccountId,
    int userId,
    DateTime? startDateUtc,
    DateTime? endDateUtc)
        {
            _logger.LogInformation(
                "Banka hesap ID {BankAccountId} için iþlemler getiriliyor (tarih filtresi), kullanýcý ID: {UserId}.",
                bankAccountId, userId);

            // Hesap gerçekten bu kullanýcýya mý ait?
            var account = await _context.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == bankAccountId && a.UserId == userId);

            if (account == null)
            {
                _logger.LogWarning(
                    "Kullanýcý ID {UserId} için Banka Hesap ID {BankAccountId} bulunamadý veya yetkisiz eriþim denemesi.",
                    userId, bankAccountId);
                return new List<BankTransaction>();
            }

            var q = _context.BankTransactions
                .Where(t => t.BankAccountId == bankAccountId);

            if (startDateUtc.HasValue)
                q = q.Where(t => t.TransactionDate >= startDateUtc.Value);
            if (endDateUtc.HasValue)
                q = q.Where(t => t.TransactionDate <= endDateUtc.Value);

            return await q
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
        public async Task<VakifBankApi.Enums.RegisterResult> RegisterUserAsync(string username,string password)
        {
            //kulanýcý adý var yok kontrol et
            //þifre uzunluðu 6 olmalý
            if (password.Length != 6)
            {
                _logger.LogWarning($"Kayýt denemesi baþarýsýz: Þifre uzunluðu 6 olmalý. Kullanýcý adý: {username}");
                return RegisterResult.PasswordLengthInvalid;  
            }

            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                _logger.LogWarning($"Kayýt denemesi baþarýsýz: '{username}' kullanýcý adý zaten mevcut.");
                return RegisterResult.UsernameAlreadyExists; 
            }
            
           
            //þifreyi hasle
            var hashedPassword = HashPassword(password);
            //yeni kullanýcý nesnesi oluþtur
            var newUser = new User
            {
                Username = username,
                PasswordHash = hashedPassword,
            };
            //veritabanýna ekle
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();//deðiþikleri kaydet

            _logger.LogInformation("Yeni kullanýcý '{Username}' baþarýyla kayýt edildi. ID :'{UserId}'",newUser.Username , newUser.Id);
            return RegisterResult.Success; 

        }
    }
}