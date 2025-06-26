using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels
{
    public class ConfirmResetVm
    {
        [Required(ErrorMessage = "Email vacibdir")]
        [EmailAddress(ErrorMessage = "Düzgün email daxil edin")]
        public string Email { get; set; }

        [Required(ErrorMessage = "OTP kodu vacibdir")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Kod 6 rəqəmli olmalıdır")]
        public string Code { get; set; }
    }
}
