using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Product
{
    public class UpdateProductDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int ManufacturerId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        // 0 = esaurito; negativo non ammesso.
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        // Stock letto dall'admin quando ha aperto il form. Se presente, Quantity viene scritta
        // solo se lo stock è ancora questo (altrimenti 409). Opzionale per compatibilità col FE (M4).
        [Range(0, int.MaxValue)]
        public int? ExpectedQuantity { get; set; }

        [Required]
        public string? FullDescription { get; set; }

        [Required]
        public int SubCategoryId { get; set; }

        [Required]
        public List<ProductImageDto>? ProductImages { get; set; }
    }
}
