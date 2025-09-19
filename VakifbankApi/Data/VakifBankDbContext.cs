using Microsoft.EntityFrameworkCore;
using VakifBankApi.Models; // Modellerimizi kullanabilmek i�in

namespace VakifBankApi.Data // Namespace'i VakifBankApi.Data olarak de�i�tirdik
{
    public class VakifBankDbContext : DbContext
    {
        public VakifBankDbContext(DbContextOptions<VakifBankDbContext> options) : base(options) { }

        // DbSet'ler, veritaban�m�zdaki tablolar� temsil eder.
        public DbSet<User> Users { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Veritaban� �emas� i�in ek konfig�rasyonlar
            // Username'in benzersiz olmas�n� sa�l�yoruz
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // AccountNumber'�n benzersiz olmas�n� sa�l�yoruz
            modelBuilder.Entity<BankAccount>()
                .HasIndex(a => a.AccountNumber)
                .IsUnique();

            // Decimal tipleri i�in SQL Server'da hassasiyet ve �l�ek belirtme (uyar�lar� gidermek i�in)
            modelBuilder.Entity<BankAccount>()
                .Property(a => a.Balance)
                .HasColumnType("decimal(18, 2)"); // Toplam 18 basamak, virg�lden sonra 2 basamak

            modelBuilder.Entity<BankTransaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18, 2)"); // Toplam 18 basamak, virg�lden sonra 2 basamak
        }
    }
}