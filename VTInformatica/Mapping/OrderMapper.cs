using VTInformatica.DTOs.Order;
using VTInformatica.DTOs.Product;
using VTInformatica.Models;

namespace VTInformatica.Mapping
{
    public static class OrderMapper
    {
        // Il prezzo mostrato è quello pagato (OrderItem.UnitPrice), non quello corrente del prodotto.
        public static GetOrderDto ToDto(Order order) => new GetOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerEmail = order.CustomerEmail,
            CreatedAt = order.CreatedAt,
            CustomerComment = order.CustomerComment,
            IsDeleted = order.IsDeleted,
            Items = order.OrderItems.Select(oi => new GetOrderItemsDto
            {
                Quantity = oi.Quantity,
                Product = new GetOrderProductDto
                {
                    Name = oi.Product.Name,
                    Price = oi.UnitPrice,
                    ProductImages = oi.Product.ProductImages != null
                        ? oi.Product.ProductImages.Select(img => new ProductImageDto
                        {
                            ImageUrl = img.ImageUrl
                        }).ToList()
                        : new List<ProductImageDto>()
                }
            }).ToList()
        };
    }
}
