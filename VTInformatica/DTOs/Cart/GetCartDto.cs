namespace VTInformatica.DTOs.Cart
{
    public class GetCartDto
    {
        public string? Email { get; set; }
        public ICollection<GetCartItemDto> Items { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
