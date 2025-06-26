using System.ComponentModel.DataAnnotations;

namespace LocatraMain.Models.Sale
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required, Range(1, 100)]
        public int Quantity { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
