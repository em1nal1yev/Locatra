using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels
{
    public class ConfirmEmailVm
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Kod boş ola bilməz.")]

        public string VerificationCode { get; set; } 
    }
}
