using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels.Sale
{
    public class ReviewCreateViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Reytinq tələb olunur.")]
        [Range(1, 5, ErrorMessage = "Reytinq 1 ilə 5 arasında olmalıdır.")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Şərh maksimum 500 simvol ola bilər.")]
        public string Comment { get; set; }
    }
}
