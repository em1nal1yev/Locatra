using System.ComponentModel.DataAnnotations;

namespace LocatraMain.Models.Sale
{
    public class Review
    {
        public int Id { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
