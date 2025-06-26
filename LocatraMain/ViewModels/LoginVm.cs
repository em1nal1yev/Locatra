using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels
{
    public class LoginVm
    {
        [Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
