using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels
{
    public class ForgotPasswordVm
    {
        [Required(ErrorMessage = "Email vacibdir")]
        [EmailAddress(ErrorMessage = "Düzgün email daxil edin")]
        public string Email { get; set; }
    }
}
