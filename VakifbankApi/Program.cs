using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using VakifBankApi.Data; // Kendi DbContext'imiz
using VakifBankApi.Models; // Kendi Modellerimiz
using VakifBankApi.Services; // Kendi MyBankService'imiz ve VakifBankApiService
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims; // ClaimTypes i�in
using System.Security.Cryptography; // SHA256 i�in
using Microsoft.Extensions.Logging;
using VakifBankApi.Enums;





var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI deste�i
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Swagger'a Bearer Token (JWT) deste�i ekleme
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// **********************************************
// DI ve Hizmet Yap�land�rmalar�
// **********************************************

// Kendi veritaban� ba�lant�s� ve EF Core'u yap�land�r (MyBankApi'den geldi)
builder.Services.AddDbContext<VakifBankDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Kendi i� i� mant��� servisimiz
builder.Services.AddScoped<MyBankService>();

// Vak�fBank API ayarlar�n� oku ve DI'ya kaydet
builder.Services.Configure<VakifBankApiSettings>(builder.Configuration.GetSection("VakifBankApiSettings"));

// ILogger ekleme (Console logger ile)
builder.Services.AddLogging(configure => configure.AddConsole());

// HttpClient ve VakifBankApiService'i DI konteynerine kaydet
builder.Services.AddHttpClient<VakifBankApiService>();

// JWT Kimlik Do�rulamas� Ekleme (Mevcut ayarlar�n�zdan geldi)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(); // Yetkilendirme servisini ekle

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// CORS ayarlar� (Angular'�n eri�imi i�in)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});


var app = builder.Build();

// **********************************************
// HTTP �stek ��lem Hatt� (Middleware) Yap�land�rmas�
// **********************************************

// Geli�tirme ortam�nda Swagger UI'� etkinle�tir
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // **********************************************
    // Veritaban�na ba�lang�� verisi ekleme (Seed Data - MyBankApi'den geldi)
    // **********************************************
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<VakifBankDbContext>(); // Kendi DbContext'imizi kullan�yoruz
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            dbContext.Database.Migrate(); // Migration'lar� otomatik uygula
            logger.LogInformation("Veritaban� migration'lar� ba�ar�yla uyguland�.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Veritaban� migration'lar� uygulan�rken hata olu�tu.");
            // Hata olursa uygulamay� ba�latmay� durdur
        }

        if (!dbContext.Users.Any())
        {
            logger.LogInformation("Varsay�lan kullan�c� ve hesaplar ekleniyor...");

            var myBankService = scope.ServiceProvider.GetRequiredService<MyBankService>();

            var defaultUser = new User
            {
                Username = "testuser",
                PasswordHash = myBankService.HashPassword("testpass")
            };
            dbContext.Users.Add(defaultUser);
            await dbContext.SaveChangesAsync();

            // Mevcut hesaplar
            var account1 = new BankAccount
            {
                UserId = defaultUser.Id,
                AccountNumber = "MYB1001",
                IBAN = "TR100000000000MYB1001",
                Balance = 1500.75M,
                CurrencyCode = "TL",
                AccountName = "Ana Hesap"
            };
            var account2 = new BankAccount
            {
                UserId = defaultUser.Id,
                AccountNumber = "MYB2002",
                IBAN = "TR100000000000MYB2002",
                Balance = 500.00M,
                CurrencyCode = "USD",
                AccountName = "Dolar Hesab�"
            };

      
            var account3 = new BankAccount
            {
                UserId = defaultUser.Id,
                AccountNumber = "MYB3003",
                IBAN = "TR100000000000MYB3003",
                Balance = 750.50M,
                CurrencyCode = "EUR",
                AccountName = "Euro Hesab�"
            };
            var account4 = new BankAccount
            {
                UserId = defaultUser.Id,
                AccountNumber = "MYB4004",
                IBAN = "TR100000000000MYB4004",
                Balance = 25000.00M,
                CurrencyCode = "TL",
                AccountName = "Birikim Hesab�"
            };
            var account5 = new BankAccount
            {
                UserId = defaultUser.Id,
                AccountNumber = "MYB5005",
                IBAN = "TR100000000000MYB5005",
                Balance = 123.45M,
                CurrencyCode = "GBP",
                AccountName = "Sterlin Hesab�"
            };

            dbContext.BankAccounts.AddRange(account1, account2, account3, account4, account5); // Yeni hesaplar� da ekliyoruz
            await dbContext.SaveChangesAsync();

            // Mevcut i�lemler
            dbContext.BankTransactions.AddRange(
                new BankTransaction { BankAccountId = account1.Id, Amount = 2000M, TransactionType = "Yat�r�m", Description = "Maa� �demesi", TransactionDate = DateTime.UtcNow.AddDays(-5) },
                new BankTransaction { BankAccountId = account1.Id, Amount = -499.25M, TransactionType = "�ekim", Description = "Market Harcamas�", TransactionDate = DateTime.UtcNow.AddDays(-2) },
                new BankTransaction { BankAccountId = account2.Id, Amount = 500M, TransactionType = "Yat�r�m", Description = "Dolar Al�m�", TransactionDate = DateTime.UtcNow.AddDays(-1) }
            );

            // YEN� EKLENECEK ��LEMLER
            dbContext.BankTransactions.AddRange(
                new BankTransaction { BankAccountId = account3.Id, Amount = 1000M, TransactionType = "Yat�r�m", Description = "Euro Hesab�na Havale", TransactionDate = DateTime.UtcNow.AddDays(-7) },
                new BankTransaction { BankAccountId = account3.Id, Amount = -250M, TransactionType = "�ekim", Description = "Online Al��veri�", TransactionDate = DateTime.UtcNow.AddDays(-3) },
                new BankTransaction { BankAccountId = account4.Id, Amount = 10000M, TransactionType = "Yat�r�m", Description = "Ek Gelir Aktar�m�", TransactionDate = DateTime.UtcNow.AddDays(-10) },
                new BankTransaction { BankAccountId = account4.Id, Amount = -5000M, TransactionType = "�ekim", Description = "Ev Pe�inat�", TransactionDate = DateTime.UtcNow.AddDays(-6) },
                new BankTransaction { BankAccountId = account5.Id, Amount = 150M, TransactionType = "Yat�r�m", Description = "Sterlin Transferi", TransactionDate = DateTime.UtcNow.AddDays(-4) },
                new BankTransaction { BankAccountId = account5.Id, Amount = -26.55M, TransactionType = "�ekim", Description = "Uygulama Aboneli�i", TransactionDate = DateTime.UtcNow.AddDays(-1) }
            );
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Varsay�lan kullan�c� (testuser) ve hesaplar/i�lemler ba�ar�yla eklendi.");
        }

    }
}

app.UseHttpsRedirection();

// UseRouting'i en �ste ta��d�m
app.UseRouting();

// CORS, Authentication'dan �nce gelmeli! Bu �ok kritik bir d�zeltme.
app.UseCors(MyAllowSpecificOrigins);

// Authentication ve Authorization, UseCors'tan sonra gelmeli
app.UseAuthentication();
app.UseAuthorization();

// **********************************************
// API Endpoint'leri
// **********************************************

// KEND� S�STEM�M�Z�N LOGIN ENDPOINT'� (Veritaban� tabanl�)
app.MapPost("/api/mybank/login", async (VakifBankApi.Models.LoginRequest request, MyBankService myBankService, IConfiguration config) =>
{
    var user = await myBankService.ValidateUser(request.Username, request.Password); // ValidateUser metodunu �a��rd�k

    if (user == null)
    {
        return Results.Unauthorized();
    }

    var jwtSettings = config.GetSection("Jwt");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullan�c�n�n DB ID'sini token'a ekle
            new Claim(ClaimTypes.Name, user.Username)
        }),
        Expires = DateTime.UtcNow.AddHours(1), // Token s�resi
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature), // SHA256 kullan�yoruz
        Issuer = jwtSettings["Issuer"],
        Audience = jwtSettings["Audience"]
    };
    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);

    return Results.Ok(new LoginResponse // LoginResponse modelini d�nd�r
    {
        Token = tokenHandler.WriteToken(token),
        UserId = user.Id,
        Username = user.Username
    });
})
.AllowAnonymous() // Bu endpoint token gerektirmez
.WithTags("MyBank API (Internal)"); // Swagger'da kendi API'm�z� gruplamak i�in yeni tag

app.MapPost("/api/mybank/register", async (VakifBankApi.Models.RegisterRequest request, MyBankService myBankService) => // namespace'i belirtmek iyi olur
{
    if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest("Kullan�c� ad� ve �ifre gereklidir.");
    }


    var result = await myBankService.RegisterUserAsync(request.Username, request.Password);
    switch(result){
        case RegisterResult.Success:
            return Results.Ok(new { Messaage = "Kullan�c� ba�ar�yla kay�t edildi" });
        case RegisterResult.UsernameAlreadyExists:
            return Results.Conflict("Bu kullan�c� ad� zaten kullan�lmaktad�r.");
        case RegisterResult.PasswordLengthInvalid:
            return Results.BadRequest("�ifre tam olarak 6 karakter uzunlu�unda olmal�d�r."); // Veya "�ifre en az 6 karakter uzunlu�unda olmal�d�r."
        default:
            return Results.Problem(statusCode: 500,
        title: "Sunucu Hatas�",
        detail: "Bilinmeyen bir hata olu�tu.");
    }
})
    .WithTags("MyBank API (Internal)")
    .AllowAnonymous();

// KEND� S�STEM�M�Z�N HESAP L�STES� ENDPOINT'�
app.MapGet("/api/mybank/accounts", async (ClaimsPrincipal userPrincipal, MyBankService myBankService) =>
{
    var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
    {
        return Results.Unauthorized(); // Token'da kullan�c� ID'si yok veya ge�ersiz
    }

    var accounts = await myBankService.GetBankAccountsByUserId(userId);
    return Results.Ok(accounts);
})
.RequireAuthorization() // JWT token gerektirir
.WithTags("MyBank API (Internal)");

// KEND� S�STEM�M�Z�N HESAP DETAYI ENDPOINT'�
app.MapGet("/api/mybank/AccountDetail/{iban}", async (string iban, ClaimsPrincipal userPrincipal, MyBankService myBankService) =>
{
    var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
    {
        return Results.Unauthorized();
    }

    var account = await myBankService.GetAccountDetailByIBAN(iban, userId);
    if (account == null)
    {
        return Results.NotFound("Hesap bulunamad� veya bu kullan�c�ya ait de�il.");
    }
    return Results.Ok(account);
})
.RequireAuthorization()
.WithTags("MyBank API (Internal)");

// KEND� S�STEM�M�Z�N ��LEM HAREKETLER� ENDPOINT'�
app.MapPost("/api/mybank/transactions", async (
    MyBankTransactionsRequest req,
    ClaimsPrincipal userPrincipal,
    MyBankService myBankService) =>
{
    // Auth zorunlu
    var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        return Results.Unauthorized();

    if (req.BankAccountId <= 0)
        return Results.BadRequest("BankAccountId zorunludur.");

    // Tarihleri UTC kabul edelim (UI taraf�nda toISOString ile g�nder)
    DateTime? startUtc = req.StartDate?.ToUniversalTime();
    DateTime? endUtc = req.EndDate?.ToUniversalTime();

    var list = await myBankService.GetTransactionsByAccountId(
        req.BankAccountId, userId, startUtc, endUtc);

    // Hi� kay�t yoksa 200 [] d�nd�rmek daha kullan��l� olur.
    return Results.Ok(list);
})
.RequireAuthorization()
.WithTags("MyBank API (Internal)");


// --- MEVCUT VAKIFBANK API ENDPOINT'LER� 

// 2. Hesap Listesi Endpoint'i (Vak�fBank API'sinden hesaplar� �eker, yetkilendirme gerektirir)
app.MapPost("/api/vakifbank/accounts", async ( VakifBankApiService vakifBankApiService) =>
{
    try
    {
        var accounts = await vakifBankApiService.GetAccountsAsync();
        if (accounts == null || accounts.Header?.StatusCode != "APIGW000000")
        {
            var problemDetails = new
            {
                Type = "https://tools.ietf.org/html/rfc7807",
                Title = "Vak�fBank Hesap Listesi Al�m Hatas�",
                Status = StatusCodes.Status400BadRequest,
                Detail = accounts?.Header?.StatusDescription ?? "Banka API's�ndan beklenmeyen veya hatal� yan�t al�nd�.",
                StatusCode = accounts?.Header?.StatusCode
            };
            return Results.Json(problemDetails, statusCode: StatusCodes.Status400BadRequest);
        }
        return Results.Ok(accounts);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError, title: "API Controller Hatas�: Hesap bilgileri al�n�rken hata olu�tu");
    }
}).RequireAuthorization()
  .WithTags("VakifBank API (External)"); // Tag'� de�i�tirdim

// Yeni Endpoint: Hesap Detay� (Vak�fBank API's�ndan �eker)
app.MapPost("/api/vakifbank/accountDetail", async (VakifBankAccountDetailRequest request, VakifBankApiService vakifBankApiService) =>
{
    try
    {
        if (request == null || string.IsNullOrEmpty(request.AccountNumber))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "AccountNumber", new[] { "Hesap numaras� zorunludur." } }
            }, "Ge�ersiz �stek Parametreleri", statusCode: StatusCodes.Status400BadRequest);
        }

        var accountDetail = await vakifBankApiService.GetAccountDetailAsync(request);

        if (accountDetail == null || accountDetail.Header?.StatusCode != "APIGW000000")
        {
            var problemDetails = new
            {
                Type = "https://tools.ietf.org/html/rfc7807",
                Title = "Vak�fBank Hesap Detay� Al�m Hatas�",
                Status = StatusCodes.Status400BadRequest,
                Detail = accountDetail?.Header?.StatusDescription ?? "Banka API's�ndan beklenmeyen veya hatal� yan�t al�nd�.",
                StatusCode = accountDetail?.Header?.StatusCode
            };
            return Results.Json(problemDetails, statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(accountDetail);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError, title: "API Controller Hatas�: Hesap detay� al�n�rken hata olu�tu");
    }
}).RequireAuthorization()
  .WithTags("VakifBank API (External)"); // Tag'� de�i�tirdim

// Yeni Endpoint: Hesap Hareketleri (Vak�fBank API's�ndan �eker)
app.MapPost("/api/vakifbank/transactions", async (VakifBankAccountTransactionsRequest request, VakifBankApiService vakifBankApiService) =>
{
    try
    {
        if (request == null || string.IsNullOrEmpty(request.AccountNumber) || string.IsNullOrEmpty(request.StartDate) || string.IsNullOrEmpty(request.EndDate))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "AccountNumber", new[] { "Hesap numaras� zorunludur." } },
                { "StartDate", new[] { "Ba�lang�� tarihi zorunludur ve 'yyyy-MM-ddTHH24:mm:ss' format�nda olmal�d�r." } },
                { "EndDate", new[] { "Biti� tarihi zorunludur ve 'yyyy-MM-ddTHH24:mm:ss' format�nda olmal�d�r." } }
            }, "Ge�ersiz �stek Parametreleri", statusCode: StatusCodes.Status400BadRequest);
        }

        var transactions = await vakifBankApiService.GetAccountTransactionsAsync(request);

        if (transactions == null || transactions.Header?.StatusCode != "APIGW000000")
        {
            var problemDetails = new
            {
                Type = "https://tools.ietf.org/html/rfc7807",
                Title = "Vak�fBank Hesap Hareketleri Al�m Hatas�",
                Status = StatusCodes.Status400BadRequest,
                Detail = transactions?.Header?.StatusDescription ?? "Banka API's�ndan beklenmeyen veya hatal� yan�t al�nd�.",
                StatusCode = transactions?.Header?.StatusCode
            };
            return Results.Json(problemDetails, statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(transactions);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError, title: "API Controller Hatas�: Hesap hareketleri al�n�rken hata olu�tu");
    }
}).RequireAuthorization()
  .WithTags("VakifBank API (External)"); // Tag'� de�i�tirdim


app.Run();