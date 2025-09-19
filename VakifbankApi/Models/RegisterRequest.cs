using System.ComponentModel.DataAnnotations;
namespace VakifBankApi.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Kullanýcý adý boþ olamaz.")]
        public required string Username { get; set; }
        [Required(ErrorMessage = "Þifre boþ olamaz.")] 
        [StringLength(6, ErrorMessage = "Þifre  6 karakter uzunluðunda olmalýdýr.")]
        public required string Password { get; set; }
    }
}