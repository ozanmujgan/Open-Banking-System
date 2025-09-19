using System.ComponentModel.DataAnnotations;
namespace VakifBankApi.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Kullan�c� ad� bo� olamaz.")]
        public required string Username { get; set; }
        [Required(ErrorMessage = "�ifre bo� olamaz.")] 
        [StringLength(6, ErrorMessage = "�ifre  6 karakter uzunlu�unda olmal�d�r.")]
        public required string Password { get; set; }
    }
}