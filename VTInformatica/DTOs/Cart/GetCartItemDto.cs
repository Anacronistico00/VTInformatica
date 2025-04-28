namespace VTInformatica.DTOs.Cart
{
    public class GetCartItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public GetCartProductDto Product { get; set; }
    }
}
