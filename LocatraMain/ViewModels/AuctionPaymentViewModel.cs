using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels
{
    public class AuctionPaymentViewModel
    {
        public int AuctionId { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Ünvan")]
        public string Address { get; set; }
    }
}
