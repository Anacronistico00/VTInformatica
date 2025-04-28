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

        public async Task<List<GetOrderDto>> GetAllAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .ToListAsync();

            return orders.Select(o => new GetOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerEmail = o.CustomerEmail,
                CreatedAt = o.CreatedAt,
                CustomerComment = o.CustomerComment,
                IsDeleted = o.IsDeleted,
                Items = o.OrderItems.Select(oi => new GetOrderItemsDto
                {
                    Quantity = oi.Quantity,
                    Product = new GetOrderProductDto()
                    {
                        Name = oi.Product.Name,
                        Price = oi.Product.Price,
                        ProductImages = oi.Product.ProductImages.Select(pi => new ProductImageDto
                        {
                            ImageUrl = pi.ImageUrl,
                        }).ToList(),
                    }
                }).ToList()
            }).ToList();
        }

        public async Task<GetOrderDto?> GetByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            return new GetOrderDto
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
                        Price = oi.Product.Price,
                        ProductImages = oi.Product.ProductImages.Select(pi => new ProductImageDto
                        {
                            ImageUrl = pi.ImageUrl
                        }).ToList()
                    }
                }).ToList()
            };

        }

        public async Task<List<GetOrderDto>> GetOrdersByEmailAsync(string email)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerEmail == email)
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
                CustomerEmail = o.CustomerEmail,
                CreatedAt = o.CreatedAt,
                CustomerComment = o.CustomerComment,
                IsDeleted = o.IsDeleted,
                Items = o.OrderItems.Select(oi => new GetOrderItemsDto
                {
                    Quantity = oi.Quantity,
                    Product = new GetOrderProductDto
                    {
                        Name = oi.Product.Name,
                        Price = oi.Product.Price,
                        ProductImages = oi.Product.ProductImages != null
                            ? oi.Product.ProductImages.Select(img => new ProductImageDto
                            {
                                ImageUrl = img.ImageUrl
                            }).ToList()
                            : new List<ProductImageDto>()
                    }
                }).ToList()
            }).ToList();
        }

        public async Task<GetOrderDto> CreateOrderFromCartAsync(string userEmail)
        {
            var cart = await _context.Carts.Include(c => c.Items)
                                            .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (cart == null || !cart.Items.Any())
            {
                throw new InvalidOperationException("Il carrello è vuoto.");
            }

            var order = new Order
            {
                CustomerId = cart.UserId,
                CustomerEmail = userEmail,
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

        public async Task<bool> RestoreAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.IsDeleted);

            if (order == null)
            {
                return false;
            }

            order.IsDeleted = false;
            _context.Orders.Update(order);

            foreach (var orderItem in order.OrderItems)
            {
                var product = orderItem.Product;
                if (product != null)
                {
                    if (product.Quantity >= orderItem.Quantity)
                    {
                        product.Quantity -= orderItem.Quantity;
                        _context.Products.Update(product);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }


    }
}
