using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.DTOs.Product;
using VTInformatica.DTOs.SubCategory;

namespace VTInformatica.DTOs.Cart
{
    public class GetCartProductDto
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public List<ProductImageDto>? ProductImages { get; set; }
    }
}
