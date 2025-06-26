using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels
{
    public class AuctionCreateVm
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, 999999)]
        public decimal StartingPrice { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Required]
        [Display(Name = "Images")]
        public List<IFormFile> Images { get; set; }
    }
}
