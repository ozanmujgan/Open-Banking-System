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
using System.Security.Claims; // ClaimTypes için
using System.Security.Cryptography; // SHA256 için
using Microsoft.Extensions.Logging;
using VakifBankApi.Enums;





var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI desteði
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Swagger'a Bearer Token (JWT) desteði ekleme
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
// DI ve Hizmet Yapýlandýrmalarý
// **********************************************

// Kendi veritabaný baðlantýsý ve EF Core'u yapýlandýr (MyBankApi'den geldi)
builder.Services.AddDbContext<VakifBankDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Kendi iç iþ mantýðý servisimiz
builder.Services.AddScoped<MyBankService>();

// VakýfBank API ayarlarýný oku ve DI'ya kaydet
builder.Services.Configure<VakifBankApiSettings>(builder.Configuration.GetSection("VakifBankApiSettings"));

// ILogger ekleme (Console logger ile)
builder.Services.AddLogging(configure => configure.AddConsole());

// HttpClient ve VakifBankApiService'i DI konteynerine kaydet
builder.Services.AddHttpClient<VakifBankApiService>();

// JWT Kimlik Doðrulamasý Ekleme (Mevcut ayarlarýnýzdan geldi)
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
// CORS ayarlarý (Angular'ýn eriþimi için)
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
// HTTP Ýstek Ýþlem Hattý (Middleware) Yapýlandýrmasý
// **********************************************

// Geliþtirme ortamýnda Swagger UI'ý etkinleþtir
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // **********************************************
    // Veritabanýna baþlangýç verisi ekleme (Seed Data - MyBankApi'den geldi)
    // **********************************************
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<VakifBankDbContext>(); // Kendi DbContext'imizi kullanýyoruz
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            dbContext.Database.Migrate(); // Migration'larý otomatik uygula
            logger.LogInformation("Veritabaný migration'larý baþarýyla uygulandý.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Veritabaný migration'larý uygulanýrken hata oluþtu.");
            // Hata olursa uygulamayý baþlatmayý durdur
        }

        if (!dbContext.Users.Any())
        {
            logger.LogInformation("Varsayýlan kullanýcý ve hesaplar ekleniyor...");

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
                AccountName = "Dolar Hesabý"
            };

      
            var account3 = new BankAccount
            {
                UserId = defaultUser.Id,
                AccountNumber = "MYB3003",
                IBAN = "TR100000000000MYB3003",
                Balance = 750.50M,
                CurrencyCode = "EUR",
                AccountName = "Euro Hesabý"
            };
            var account4 = new BankAccount
            {
                UserId = defaultUser.Id,
                AccountNumber = "MYB4004",
                IBAN = "TR100000000000MYB4004",
                Balance = 25000.00M,
                CurrencyCode = "TL",
                AccountName = "Birikim Hesabý"
            };
            var account5 = new BankAccount
            {
                UserId = defaultUser.Id,
                AccountNumber = "MYB5005",
                IBAN = "TR100000000000MYB5005",
                Balance = 123.45M,
                CurrencyCode = "GBP",
                AccountName = "Sterlin Hesabý"
            };

            dbContext.BankAccounts.AddRange(account1, account2, account3, account4, account5); // Yeni hesaplarý da ekliyoruz
            await dbContext.SaveChangesAsync();

            // Mevcut iþlemler
            dbContext.BankTransactions.AddRange(
                new BankTransaction { BankAccountId = account1.Id, Amount = 2000M, TransactionType = "Yatýrým", Description = "Maaþ Ödemesi", TransactionDate = DateTime.UtcNow.AddDays(-5) },
                new BankTransaction { BankAccountId = account1.Id, Amount = -499.25M, TransactionType = "Çekim", Description = "Market Harcamasý", TransactionDate = DateTime.UtcNow.AddDays(-2) },
                new BankTransaction { BankAccountId = account2.Id, Amount = 500M, TransactionType = "Yatýrým", Description = "Dolar Alýmý", TransactionDate = DateTime.UtcNow.AddDays(-1) }
            );

            // YENÝ EKLENECEK ÝÞLEMLER
            dbContext.BankTransactions.AddRange(
                new BankTransaction { BankAccountId = account3.Id, Amount = 1000M, TransactionType = "Yatýrým", Description = "Euro Hesabýna Havale", TransactionDate = DateTime.UtcNow.AddDays(-7) },
                new BankTransaction { BankAccountId = account3.Id, Amount = -250M, TransactionType = "Çekim", Description = "Online Alýþveriþ", TransactionDate = DateTime.UtcNow.AddDays(-3) },
                new BankTransaction { BankAccountId = account4.Id, Amount = 10000M, TransactionType = "Yatýrým", Description = "Ek Gelir Aktarýmý", TransactionDate = DateTime.UtcNow.AddDays(-10) },
                new BankTransaction { BankAccountId = account4.Id, Amount = -5000M, TransactionType = "Çekim", Description = "Ev Peþinatý", TransactionDate = DateTime.UtcNow.AddDays(-6) },
                new BankTransaction { BankAccountId = account5.Id, Amount = 150M, TransactionType = "Yatýrým", Description = "Sterlin Transferi", TransactionDate = DateTime.UtcNow.AddDays(-4) },
                new BankTransaction { BankAccountId = account5.Id, Amount = -26.55M, TransactionType = "Çekim", Description = "Uygulama Aboneliði", TransactionDate = DateTime.UtcNow.AddDays(-1) }
            );
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Varsayýlan kullanýcý (testuser) ve hesaplar/iþlemler baþarýyla eklendi.");
        }

    }
}

app.UseHttpsRedirection();

// UseRouting'i en üste taþýdým
app.UseRouting();

// CORS, Authentication'dan önce gelmeli! Bu çok kritik bir düzeltme.
app.UseCors(MyAllowSpecificOrigins);

// Authentication ve Authorization, UseCors'tan sonra gelmeli
app.UseAuthentication();
app.UseAuthorization();

// **********************************************
// API Endpoint'leri
// **********************************************

// KENDÝ SÝSTEMÝMÝZÝN LOGIN ENDPOINT'Ý (Veritabaný tabanlý)
app.MapPost("/api/mybank/login", async (VakifBankApi.Models.LoginRequest request, MyBankService myBankService, IConfiguration config) =>
{
    var user = await myBankService.ValidateUser(request.Username, request.Password); // ValidateUser metodunu çaðýrdýk

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
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanýcýnýn DB ID'sini token'a ekle
            new Claim(ClaimTypes.Name, user.Username)
        }),
        Expires = DateTime.UtcNow.AddHours(1), // Token süresi
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature), // SHA256 kullanýyoruz
        Issuer = jwtSettings["Issuer"],
        Audience = jwtSettings["Audience"]
    };
    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);

    return Results.Ok(new LoginResponse // LoginResponse modelini döndür
    {
        Token = tokenHandler.WriteToken(token),
        UserId = user.Id,
        Username = user.Username
    });
})
.AllowAnonymous() // Bu endpoint token gerektirmez
.WithTags("MyBank API (Internal)"); // Swagger'da kendi API'mýzý gruplamak için yeni tag

app.MapPost("/api/mybank/register", async (VakifBankApi.Models.RegisterRequest request, MyBankService myBankService) => // namespace'i belirtmek iyi olur
{
    if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest("Kullanýcý adý ve þifre gereklidir.");
    }


    var result = await myBankService.RegisterUserAsync(request.Username, request.Password);
    switch(result){
        case RegisterResult.Success:
            return Results.Ok(new { Messaage = "Kullanýcý baþarýyla kayýt edildi" });
        case RegisterResult.UsernameAlreadyExists:
            return Results.Conflict("Bu kullanýcý adý zaten kullanýlmaktadýr.");
        case RegisterResult.PasswordLengthInvalid:
            return Results.BadRequest("Þifre tam olarak 6 karakter uzunluðunda olmalýdýr."); // Veya "Þifre en az 6 karakter uzunluðunda olmalýdýr."
        default:
            return Results.Problem(statusCode: 500,
        title: "Sunucu Hatasý",
        detail: "Bilinmeyen bir hata oluþtu.");
    }
})
    .WithTags("MyBank API (Internal)")
    .AllowAnonymous();

// KENDÝ SÝSTEMÝMÝZÝN HESAP LÝSTESÝ ENDPOINT'Ý
app.MapGet("/api/mybank/accounts", async (ClaimsPrincipal userPrincipal, MyBankService myBankService) =>
{
    var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
    {
        return Results.Unauthorized(); // Token'da kullanýcý ID'si yok veya geçersiz
    }

    var accounts = await myBankService.GetBankAccountsByUserId(userId);
    return Results.Ok(accounts);
})
.RequireAuthorization() // JWT token gerektirir
.WithTags("MyBank API (Internal)");

// KENDÝ SÝSTEMÝMÝZÝN HESAP DETAYI ENDPOINT'Ý
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
        return Results.NotFound("Hesap bulunamadý veya bu kullanýcýya ait deðil.");
    }
    return Results.Ok(account);
})
.RequireAuthorization()
.WithTags("MyBank API (Internal)");

// KENDÝ SÝSTEMÝMÝZÝN ÝÞLEM HAREKETLERÝ ENDPOINT'Ý
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

    // Tarihleri UTC kabul edelim (UI tarafýnda toISOString ile gönder)
    DateTime? startUtc = req.StartDate?.ToUniversalTime();
    DateTime? endUtc = req.EndDate?.ToUniversalTime();

    var list = await myBankService.GetTransactionsByAccountId(
        req.BankAccountId, userId, startUtc, endUtc);

    // Hiç kayýt yoksa 200 [] döndürmek daha kullanýþlý olur.
    return Results.Ok(list);
})
.RequireAuthorization()
.WithTags("MyBank API (Internal)");


// --- MEVCUT VAKIFBANK API ENDPOINT'LERÝ 

// 2. Hesap Listesi Endpoint'i (VakýfBank API'sinden hesaplarý çeker, yetkilendirme gerektirir)
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
                Title = "VakýfBank Hesap Listesi Alým Hatasý",
                Status = StatusCodes.Status400BadRequest,
                Detail = accounts?.Header?.StatusDescription ?? "Banka API'sýndan beklenmeyen veya hatalý yanýt alýndý.",
                StatusCode = accounts?.Header?.StatusCode
            };
            return Results.Json(problemDetails, statusCode: StatusCodes.Status400BadRequest);
        }
        return Results.Ok(accounts);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError, title: "API Controller Hatasý: Hesap bilgileri alýnýrken hata oluþtu");
    }
}).RequireAuthorization()
  .WithTags("VakifBank API (External)"); // Tag'ý deðiþtirdim

// Yeni Endpoint: Hesap Detayý (VakýfBank API'sýndan çeker)
app.MapPost("/api/vakifbank/accountDetail", async (VakifBankAccountDetailRequest request, VakifBankApiService vakifBankApiService) =>
{
    try
    {
        if (request == null || string.IsNullOrEmpty(request.AccountNumber))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "AccountNumber", new[] { "Hesap numarasý zorunludur." } }
            }, "Geçersiz Ýstek Parametreleri", statusCode: StatusCodes.Status400BadRequest);
        }

        var accountDetail = await vakifBankApiService.GetAccountDetailAsync(request);

        if (accountDetail == null || accountDetail.Header?.StatusCode != "APIGW000000")
        {
            var problemDetails = new
            {
                Type = "https://tools.ietf.org/html/rfc7807",
                Title = "VakýfBank Hesap Detayý Alým Hatasý",
                Status = StatusCodes.Status400BadRequest,
                Detail = accountDetail?.Header?.StatusDescription ?? "Banka API'sýndan beklenmeyen veya hatalý yanýt alýndý.",
                StatusCode = accountDetail?.Header?.StatusCode
            };
            return Results.Json(problemDetails, statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(accountDetail);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError, title: "API Controller Hatasý: Hesap detayý alýnýrken hata oluþtu");
    }
}).RequireAuthorization()
  .WithTags("VakifBank API (External)"); // Tag'ý deðiþtirdim

// Yeni Endpoint: Hesap Hareketleri (VakýfBank API'sýndan çeker)
app.MapPost("/api/vakifbank/transactions", async (VakifBankAccountTransactionsRequest request, VakifBankApiService vakifBankApiService) =>
{
    try
    {
        if (request == null || string.IsNullOrEmpty(request.AccountNumber) || string.IsNullOrEmpty(request.StartDate) || string.IsNullOrEmpty(request.EndDate))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "AccountNumber", new[] { "Hesap numarasý zorunludur." } },
                { "StartDate", new[] { "Baþlangýç tarihi zorunludur ve 'yyyy-MM-ddTHH24:mm:ss' formatýnda olmalýdýr." } },
                { "EndDate", new[] { "Bitiþ tarihi zorunludur ve 'yyyy-MM-ddTHH24:mm:ss' formatýnda olmalýdýr." } }
            }, "Geçersiz Ýstek Parametreleri", statusCode: StatusCodes.Status400BadRequest);
        }

        var transactions = await vakifBankApiService.GetAccountTransactionsAsync(request);

        if (transactions == null || transactions.Header?.StatusCode != "APIGW000000")
        {
            var problemDetails = new
            {
                Type = "https://tools.ietf.org/html/rfc7807",
                Title = "VakýfBank Hesap Hareketleri Alým Hatasý",
                Status = StatusCodes.Status400BadRequest,
                Detail = transactions?.Header?.StatusDescription ?? "Banka API'sýndan beklenmeyen veya hatalý yanýt alýndý.",
                StatusCode = transactions?.Header?.StatusCode
            };
            return Results.Json(problemDetails, statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(transactions);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError, title: "API Controller Hatasý: Hesap hareketleri alýnýrken hata oluþtu");
    }
}).RequireAuthorization()
  .WithTags("VakifBank API (External)"); // Tag'ý deðiþtirdim


app.Run();