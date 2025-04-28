namespace VTInformatica.DTOs.Order
{
    public class GetOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CustomerComment { get; set; }
        public bool IsDeleted { get; set; }
        public List<GetOrderItemsDto> Items { get; set; }
    }
}
