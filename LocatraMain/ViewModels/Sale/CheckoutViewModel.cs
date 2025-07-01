using LocatraMain.Models.Sale;
using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels.Sale
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; }

        [Required(ErrorMessage = "Ünvan tələb olunur")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Kart nömrəsi tələb olunur")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Bitmə tarixi tələb olunur")]
        public string Expiry { get; set; }

        [Required(ErrorMessage = "CVC tələb olunur")]
        public string Cvc { get; set; }
    }
}
