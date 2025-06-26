using System.ComponentModel.DataAnnotations;

namespace LocatraMain.ViewModels.Sale
{
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Ad daxil edilməlidir.")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Açıqlama daxil edilməlidir.")]
        [StringLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Qiymət daxil edilməlidir.")]
        [Range(0.01, 10000, ErrorMessage = "Qiymət 0.01 ilə 10000 arasında olmalıdır.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Ən azı bir şəkil seçin.")]
        public List<IFormFile> Images { get; set; }
    }
}
