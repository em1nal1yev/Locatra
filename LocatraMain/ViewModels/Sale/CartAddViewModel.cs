using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels.Sale
{
    public class CartAddViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required, Range(1, 100)]
        public int Quantity { get; set; }
    }
}
