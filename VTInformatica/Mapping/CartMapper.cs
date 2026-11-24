using VTInformatica.DTOs.Cart;
using VTInformatica.DTOs.Product;
using VTInformatica.Models;

namespace VTInformatica.Mapping
{
    public static class CartMapper
    {
        // Il totale non è salvato: si calcola dalle righe e dai prezzi correnti.
        public static GetCartDto ToDto(Cart cart) => new GetCartDto
        {
            Email = cart.Email,
            TotalPrice = cart.Items
                .Where(i => i.Product != null)
                .Sum(i => i.Quantity * i.Product.Price),
            Items = cart.Items.Select(i => new GetCartItemDto
            {
                Id = i.Id,
                Quantity = i.Quantity,
                Product = new GetCartProductDto
                {
                    Name = i.Product.Name,
                    Price = i.Product.Price,
                    ProductImages = i.Product.ProductImages?.Select(img => new ProductImageDto
                    {
                        ImageUrl = img.ImageUrl
                    }).ToList()
                }
            }).ToList()
        };

        public static GetCartDto Empty(string email) => new GetCartDto
        {
            Email = email,
            TotalPrice = 0m,
            Items = new List<GetCartItemDto>()
        };
    }
}
