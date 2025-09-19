using Microsoft.EntityFrameworkCore;
using VakifBankApi.Data; // Kendi DbContext'imizi kullanabilmek i�in
using VakifBankApi.Models; // Modellerimizi kullanabilmek i�in
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using VakifBankApi.Enums;
namespace VakifBankApi.Services 
{
    // Bu s�n�f, kendi veritaban�m�zdaki (VakifBankDb) i�lemlerden sorumlu olacak
    public class MyBankService
    {
        private readonly VakifBankDbContext _context; // Kendi DbContext'imizi kullan�yoruz
        private readonly ILogger<MyBankService> _logger;

        public MyBankService(VakifBankDbContext context, ILogger<MyBankService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> ValidateUser(string username, string password) // Metot ad�n� ValidateUser olarak de�i�tirdim
        {
            _logger.LogInformation("Kullan�c� do�rulama denemesi: Kullan�c� ad� - {Username}", username);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                _logger.LogWarning("Kullan�c� do�rulama ba�ar�s�z: Kullan�c� '{Username}' bulunamad�.", username);
                return null;
            }

            var hashedPassword = HashPassword(password);

            if (hashedPassword != user.PasswordHash)
            {
                _logger.LogWarning("Kullan�c� do�rulama ba�ar�s�z: '{Username}' i�in ge�ersiz �ifre.", username);
                return null;
            }

            _logger.LogInformation("Kullan�c� do�rulama ba�ar�l�: Kullan�c� '{Username}' do�ruland�.", username);
            return user;
        }

        // �ifre Hash'leme Yard�mc� Metodu
        public string HashPassword(string password) // Public yapt�m, istersen Program.cs'de de �a��rabiliriz
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
            _logger.LogInformation("Kullan�c� ID {UserId} i�in banka hesaplar� getiriliyor.", userId);
            return await _context.BankAccounts
                                 .Where(a => a.UserId == userId)
                                 .ToListAsync();
        }

        // Hesap Detay Metodu (IBAN'a g�re, kullan�c�n�n kendi hesab� oldu�unu kontrol etmeli)
        public async Task<BankAccount?> GetAccountDetailByIBAN(string iban, int userId)
        {
            _logger.LogInformation("IBAN {IBAN} i�in hesap detay� getiriliyor, kullan�c� ID: {UserId}.", iban, userId);
            return await _context.BankAccounts
                                 .FirstOrDefaultAsync(a => a.IBAN == iban && a.UserId == userId);
        }

        // ��lem Listesi Metodu (Hesap ID'sine g�re, kullan�c�n�n kendi hesab� oldu�unu kontrol etmeli)
        public async Task<List<BankTransaction>> GetTransactionsByAccountId(
    int bankAccountId,
    int userId,
    DateTime? startDateUtc,
    DateTime? endDateUtc)
        {
            _logger.LogInformation(
                "Banka hesap ID {BankAccountId} i�in i�lemler getiriliyor (tarih filtresi), kullan�c� ID: {UserId}.",
                bankAccountId, userId);

            // Hesap ger�ekten bu kullan�c�ya m� ait?
            var account = await _context.BankAccounts
                .FirstOrDefaultAsync(a => a.Id == bankAccountId && a.UserId == userId);

            if (account == null)
            {
                _logger.LogWarning(
                    "Kullan�c� ID {UserId} i�in Banka Hesap ID {BankAccountId} bulunamad� veya yetkisiz eri�im denemesi.",
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
            //kulan�c� ad� var yok kontrol et
            //�ifre uzunlu�u 6 olmal�
            if (password.Length != 6)
            {
                _logger.LogWarning($"Kay�t denemesi ba�ar�s�z: �ifre uzunlu�u 6 olmal�. Kullan�c� ad�: {username}");
                return RegisterResult.PasswordLengthInvalid;  
            }

            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                _logger.LogWarning($"Kay�t denemesi ba�ar�s�z: '{username}' kullan�c� ad� zaten mevcut.");
                return RegisterResult.UsernameAlreadyExists; 
            }
            
           
            //�ifreyi hasle
            var hashedPassword = HashPassword(password);
            //yeni kullan�c� nesnesi olu�tur
            var newUser = new User
            {
                Username = username,
                PasswordHash = hashedPassword,
            };
            //veritaban�na ekle
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();//de�i�ikleri kaydet

            _logger.LogInformation("Yeni kullan�c� '{Username}' ba�ar�yla kay�t edildi. ID :'{UserId}'",newUser.Username , newUser.Id);
            return RegisterResult.Success; 

        }
    }
}