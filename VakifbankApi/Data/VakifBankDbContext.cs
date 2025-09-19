using Microsoft.EntityFrameworkCore;
using VakifBankApi.Models; // Modellerimizi kullanabilmek için

namespace VakifBankApi.Data // Namespace'i VakifBankApi.Data olarak deðiþtirdik
{
    public class VakifBankDbContext : DbContext
    {
        public VakifBankDbContext(DbContextOptions<VakifBankDbContext> options) : base(options) { }

        // DbSet'ler, veritabanýmýzdaki tablolarý temsil eder.
        public DbSet<User> Users { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Veritabaný þemasý için ek konfigürasyonlar
            // Username'in benzersiz olmasýný saðlýyoruz
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // AccountNumber'ýn benzersiz olmasýný saðlýyoruz
            modelBuilder.Entity<BankAccount>()
                .HasIndex(a => a.AccountNumber)
                .IsUnique();

            // Decimal tipleri için SQL Server'da hassasiyet ve ölçek belirtme (uyarýlarý gidermek için)
            modelBuilder.Entity<BankAccount>()
                .Property(a => a.Balance)
                .HasColumnType("decimal(18, 2)"); // Toplam 18 basamak, virgülden sonra 2 basamak

            modelBuilder.Entity<BankTransaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18, 2)"); // Toplam 18 basamak, virgülden sonra 2 basamak
        }
    }
}