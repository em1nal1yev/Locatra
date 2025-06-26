using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels.Sale
{
    public class WishlistAddViewModel
    {
        [Required]
        public int ProductId { get; set; }
    }
}
