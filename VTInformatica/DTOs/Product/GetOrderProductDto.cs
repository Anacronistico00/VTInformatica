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

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public List<ProductImageDto>? ProductImages { get; set; }

    }
}
