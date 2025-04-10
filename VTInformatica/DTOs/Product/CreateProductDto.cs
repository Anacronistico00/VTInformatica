using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Product
{
    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int ManufacturerId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string? FullDescription { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        [Required]
        public List<ProductImageDto>? ProductImages { get; set; }
    }
}
