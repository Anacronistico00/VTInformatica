namespace VTInformatica.DTOs.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CustomerComment { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
