using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Cart
{
    public class CartItemDto
    {
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La quantità deve essere maggiore di zero.")]
        public int Quantity { get; set; }
    }
}
