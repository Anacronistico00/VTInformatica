using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VTInformatica.DTOs.Product;

namespace VTInformatica.DTOs.Order
{
    public class GetOrderItemsDto
    {
        [Required]
        public int Quantity { get; set; }

        public GetOrderProductDto Product { get; set; }
    }
}
