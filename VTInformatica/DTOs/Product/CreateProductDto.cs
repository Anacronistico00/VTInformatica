using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Product
{
    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public int ManufacturerId { get; set; }

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        // 0 = esaurito; negativo non ammesso.
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public string? FullDescription { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        [Required]
        public List<ProductImageDto>? ProductImages { get; set; }
    }
}
