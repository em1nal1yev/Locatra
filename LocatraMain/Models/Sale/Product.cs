using System.ComponentModel.DataAnnotations;

namespace LocatraMain.Models.Sale
{
    public enum ProductStatus
    {
        Pending,
        Active,
        Rejected
    }
    public class Product
    {
        public int Id { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Pending;

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; }

        [Required, Range(0.01, 10000)]
        public decimal Price { get; set; }

        // Əvvəlki tək şəkil silinir, yerinə çoxlu şəkil gəlir
        public ICollection<ProductImage> Images { get; set; }

        public ICollection<Review> Reviews { get; set; }

        public string CreatedById { get; set; }
        public ApplicationUser CreatedBy { get; set; }
    }

}
