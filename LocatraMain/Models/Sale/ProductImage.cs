using System.ComponentModel.DataAnnotations;

namespace LocatraMain.Models.Sale
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        // Xarici açar
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
