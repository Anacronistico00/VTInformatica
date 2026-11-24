using System.ComponentModel.DataAnnotations;

namespace VTInformatica.DTOs.Order
{
    /// <summary>Corpo facoltativo del checkout: nota del cliente sull'ordine.</summary>
    public class CreateOrderFromCartDto
    {
        [StringLength(500)]
        public string? CustomerComment { get; set; }
    }
}
