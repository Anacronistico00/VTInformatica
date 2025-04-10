using Microsoft.EntityFrameworkCore;
using VTInformatica.Data;
using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.DTOs.Order;
using VTInformatica.DTOs.Product;
using VTInformatica.DTOs.Review;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Interfaces;
using VTInformatica.Models;

namespace VTInformatica.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDto>> GetAllAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            return orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CreatedAt = o.CreatedAt,
                CustomerComment = o.CustomerComment,
                Items = o.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity
                }).ToList()
            }).ToList();
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (order == null) return null;

            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                CreatedAt = order.CreatedAt,
                CustomerComment = order.CustomerComment,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity
                }).ToList()
            };
        }

        public async Task<List<GetOrderDto>> GetOrdersByUserIdAsync(string userId)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerId == userId && !o.IsDeleted)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Manufacturer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Category)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.SubCategory)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Reviews)
                .ToListAsync();

            return orders.Select(o => new GetOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CreatedAt = o.CreatedAt,
                CustomerComment = o.CustomerComment,
                Items = o.OrderItems.Select(oi => new GetOrderItemsDto
                {
                    Quantity = oi.Quantity,
                    Product = new GetOrderProductDto
                    {
                        Id = oi.Product.Id,
                        Name = oi.Product.Name,
                        Description = oi.Product.Description,
                        FullDescription = oi.Product.FullDescription,
                        Price = oi.Product.Price,
                        Quantity = oi.Product.Quantity,
                        ManufacturerId = oi.Product.ManufacturerId,
                        Manufacturer = oi.Product.Manufacturer != null ? new ManufacturerDto
                        {
                            ManufacturerName = oi.Product.Manufacturer.ManufacturerName,
                            ManufacturerLogo = oi.Product.Manufacturer.ManufacturerLogo
                        } : null,
                        Category = oi.Product.Category != null ? new CategoryDto
                        {
                            Id = oi.Product.Category.Id,
                            Name = oi.Product.Category.Name
                        } : null,
                        SubCategory = oi.Product.SubCategory != null ? new SubCategoryDto
                        {
                            Id = oi.Product.SubCategory.Id,
                            Name = oi.Product.SubCategory.Name,
                            CategoryId = oi.Product.SubCategory.CategoryId
                        } : null,
                        ProductImages = oi.Product.ProductImages != null
                            ? oi.Product.ProductImages.Select(img => new ProductImageDto
                            {
                                ImageUrl = img.ImageUrl
                            }).ToList()
                            : new List<ProductImageDto>(),
                        Reviews = oi.Product.Reviews != null
                            ? oi.Product.Reviews.Select(r => new ReviewDto
                            {
                                ReviewId = r.ReviewId,
                                ReviewRating = r.ReviewRating,
                                ReviewComment = r.ReviewComment,
                                CreatedAt = r.CreatedAt,
                                UserId = r.UserId,
                                ProductId = r.ProductId
                            }).ToList()
                            : new List<ReviewDto>()
                    }
                }).ToList()
            }).ToList();
        }

        public async Task<OrderDto> CreateOrderFromCartAsync(string userId)
        {
            var cart = await _context.Carts.Include(c => c.Items)
                                            .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                throw new InvalidOperationException("Il carrello è vuoto.");
            }

            var order = new Order
            {
                CustomerId = userId,
                CustomerComment = "Ordine creato dal carrello",
                OrderItems = cart.Items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            };

            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product == null || product.Quantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Prodotto {item.ProductId} non disponibile in quantità sufficiente.");
                }

                product.Quantity -= item.Quantity;

                _context.Products.Update(product);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(order.Id);
        }


        public async Task<bool> SoftDeleteAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && !o.IsDeleted);

            if (order == null)
            {
                return false;
            }

            order.IsDeleted = true;
            _context.Orders.Update(order);

            foreach (var orderItem in order.OrderItems)
            {
                var product = orderItem.Product;
                if (product != null)
                {
                    product.Quantity += orderItem.Quantity;
                    _context.Products.Update(product);
                }
            }

            await _context.SaveChangesAsync();

            return true;
        }

    }
}
