using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.DTOs.Review;
using VTInformatica.DTOs.SubCategory;

namespace VTInformatica.DTOs.Product
{
    public class GetOrderProductDto
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int ManufacturerId { get; set; }

        [ForeignKey(nameof(ManufacturerId))]
        public ManufacturerDto? Manufacturer { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string? FullDescription { get; set; }

        [Required]

        public CategoryDto? Category { get; set; }

        [Required]

        public SubCategoryDto? SubCategory { get; set; }

        [Required]
        public List<ProductImageDto>? ProductImages { get; set; }
        public List<ReviewDto>? Reviews { get; set; }
    }
}
