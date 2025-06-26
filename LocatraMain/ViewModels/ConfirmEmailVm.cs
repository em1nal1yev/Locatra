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

        public string VerificationCode { get; set; } // istifadəçi bu kodu daxil edəcək
    }
}
