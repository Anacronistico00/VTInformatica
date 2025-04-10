namespace VTInformatica.DTOs.Order
{
    public class GetOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CustomerComment { get; set; }
        public List<GetOrderItemsDto> Items { get; set; }
    }
}
