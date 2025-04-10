namespace VTInformatica.DTOs.Cart
{
    public class CartDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public ICollection<CartItemDto> Items { get; set; }
        public decimal? TotalPrice { get; set; }
    }
}
