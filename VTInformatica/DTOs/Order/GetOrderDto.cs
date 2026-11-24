namespace VTInformatica.DTOs.Order
{
    public class GetOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? CustomerComment { get; set; }
        public bool IsDeleted { get; set; }
        public List<GetOrderItemsDto> Items { get; set; } = new List<GetOrderItemsDto>();
    }
}
