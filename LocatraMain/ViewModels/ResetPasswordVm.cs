using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels
{
    public class ResetPasswordVm
    {
        [Required(ErrorMessage = "Email vacibdir")]
        [EmailAddress(ErrorMessage = "Düzgün email daxil edin")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Yeni parol vacibdir")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Parol ən az 8 simvol olmalıdır")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Parolu təkrar daxil edin")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Parollar uyğun deyil")]
        public string ConfirmPassword { get; set; }
    }
}
