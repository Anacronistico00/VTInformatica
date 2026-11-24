using Microsoft.EntityFrameworkCore;
using VTInformatica.Data;
using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.DTOs.Order;
using VTInformatica.DTOs.Product;
using VTInformatica.DTOs.Review;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Exceptions;
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
            // IgnoreQueryFilters: lo storico include anche i prodotti eliminati (soft delete).
            var orders = await _context.Orders
                .IgnoreQueryFilters()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .ToListAsync();

            return orders.Select(ToDto).ToList();
        }

        public async Task<GetOrderDto?> GetByIdAsync(int id)
        {
            var order = await _context.Orders
                .IgnoreQueryFilters()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return null;

            return ToDto(order);
        }

        public async Task<List<GetOrderDto>> GetOrdersByEmailAsync(string email)
        {
            var orders = await _context.Orders
                .IgnoreQueryFilters()
                .Where(o => o.CustomerEmail == email)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Manufacturer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.SubCategory)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Reviews)
                .ToListAsync();

            return orders.Select(ToDto).ToList();
        }

        // Unico punto di mapping Order -> GetOrderDto (prima duplicato in tre metodi).
        private static GetOrderDto ToDto(Order order) => new GetOrderDto
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

        public async Task<GetOrderDto> CreateOrderFromCartAsync(string userEmail, string? customerComment = null)
        {
            var cart = await _context.Carts.Include(c => c.Items)
                                            .FirstOrDefaultAsync(c => c.Email == userEmail);

            if (cart == null || !cart.Items.Any())
            {
                throw new EmptyCartException();
            }

            // AUDIT #7: ordine, stock e svuotamento carrello sono tutto o niente.
            // Se un'eccezione esce prima del Commit, il dispose fa rollback.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var order = new Order
            {
                CustomerId = cart.UserId,
                CustomerEmail = userEmail,
                CustomerComment = string.IsNullOrWhiteSpace(customerComment) ? null : customerComment.Trim(),
                OrderItems = new List<OrderItem>()
            };

            // Prezzi letti senza tracking: lo stock non passa dal change tracker.
            var productIds = cart.Items.Select(i => i.ProductId).ToList();
            var prices = await _context.Products.AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Price);

            // AUDIT #7: decremento atomico condizionale (UPDATE ... WHERE Quantity >= qty).
            // Ordine per ProductId: transazioni concorrenti bloccano le righe nello stesso ordine (niente deadlock).
            foreach (var item in cart.Items.OrderBy(i => i.ProductId))
            {
                if (!prices.TryGetValue(item.ProductId, out var unitPrice))
                {
                    throw new ProductNotFoundException(item.ProductId);
                }

                var updated = await _context.Products
                    .Where(p => p.Id == item.ProductId && p.Quantity >= item.Quantity)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, p => p.Quantity - item.Quantity));

                if (updated == 0)
                {
                    // Il dispose della transazione annulla anche i decrementi già fatti.
                    throw new InsufficientStockException(item.ProductId);
                }

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return await GetByIdAsync(order.Id);
        }


        // Annulla/ripristina: flag dell'ordine e stock nella stessa transazione, tutto o niente.
        // Il flag cambia con UPDATE condizionale (WHERE IsDeleted = atteso): due richieste uguali
        // non rimettono/tolgono lo stock due volte. Lo stock si aggiorna con UPDATE atomici
        // (Quantity ± n), in ordine di ProductId, senza leggere-e-riscrivere il valore.
        public async Task<bool> SoftDeleteAsync(int orderId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (!await SetDeletedFlagAsync(orderId, expectedDeleted: false))
            {
                return false;
            }

            foreach (var (productId, quantity) in await GetQuantitiesByProductAsync(orderId))
            {
                await _context.Products.IgnoreQueryFilters()
                    .Where(p => p.Id == productId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, p => p.Quantity + quantity));
            }

            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int orderId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            if (!await SetDeletedFlagAsync(orderId, expectedDeleted: true))
            {
                return false;
            }

            foreach (var (productId, quantity) in await GetQuantitiesByProductAsync(orderId))
            {
                var updated = await _context.Products.IgnoreQueryFilters()
                    .Where(p => p.Id == productId && p.Quantity >= quantity)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, p => p.Quantity - quantity));

                if (updated == 0)
                {
                    // 409: il dispose della transazione annulla anche il flag e i decrementi già fatti.
                    throw new InsufficientStockException(productId);
                }
            }

            await transaction.CommitAsync();
            return true;
        }

        private async Task<bool> SetDeletedFlagAsync(int orderId, bool expectedDeleted) =>
            await _context.Orders.IgnoreQueryFilters()
                .Where(o => o.Id == orderId && o.IsDeleted == expectedDeleted)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.IsDeleted, !expectedDeleted)) > 0;

        private async Task<List<(int ProductId, int Quantity)>> GetQuantitiesByProductAsync(int orderId)
        {
            var rows = await _context.Set<OrderItem>().IgnoreQueryFilters().AsNoTracking()
                .Where(oi => oi.OrderId == orderId)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(oi => oi.Quantity) })
                .OrderBy(r => r.ProductId)
                .ToListAsync();

            return rows.Select(r => (r.ProductId, r.Quantity)).ToList();
        }


    }
}
