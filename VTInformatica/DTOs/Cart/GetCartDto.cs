namespace VTInformatica.DTOs.Cart
{
    public class GetCartDto
    {
        public string? Email { get; set; }
        public ICollection<GetCartItemDto> Items { get; set; } = new List<GetCartItemDto>();
        public decimal? TotalPrice { get; set; }
    }
}
