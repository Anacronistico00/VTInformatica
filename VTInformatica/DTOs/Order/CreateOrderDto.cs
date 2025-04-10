namespace VTInformatica.DTOs.Order
{
    public class CreateOrderDto
    {
        public string CustomerId { get; set; }
        public string? CustomerComment { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
