using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VTInformatica.Data;
using VTInformatica.DTOs.Cart;
using VTInformatica.DTOs.Product;
using VTInformatica.Interfaces;
using VTInformatica.Models;
using VTInformatica.Models.Auth;

namespace VTInformatica.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<GetCartDto?> GetCartByEmailAsync(string email)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.Email == email);

            var loggedInUser = await _userManager.FindByEmailAsync(email);

            if (loggedInUser == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var userId = loggedInUser.Id;

            if (cart == null)
            {
                cart = new Cart { Email = email, Items = new List<CartItem>(), UserId = userId };
                _context.Carts.Add(cart);
            }

            var totalPrice = cart.Items
                .Where(i => i.Product != null)
                .Sum(i => i.Quantity * i.Product.Price);

            return new GetCartDto
            {
                Email = cart.Email,
                TotalPrice = totalPrice,
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
        }


        public async Task<GetCartDto> AddItemAsync(string email, CartItemDto itemDto)
        {
            var loggedInUser = await _userManager.FindByEmailAsync(email);

            if (loggedInUser == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var userId = loggedInUser.Id;

            var cart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Email == email);

            if (cart == null)
            {
                cart = new Cart { Email = email, Items = new List<CartItem>(), UserId = userId };
                _context.Carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == itemDto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += itemDto.Quantity;
                _context.Entry(existingItem).State = EntityState.Modified;
            }
            else
            {
                var item = new CartItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity
                };
                cart.Items.Add(item);
            }
            await _context.SaveChangesAsync();

            var updatedCart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.Email == email);

            var totalPrice = updatedCart.Items
                .Where(i => i.Product != null)
                .Sum(i => i.Quantity * i.Product.Price);

            updatedCart.TotalPrice = totalPrice;

            await _context.SaveChangesAsync();

            return await GetCartByEmailAsync(email);
        }

        public async Task<bool> RemoveItemAsync(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveItemQuantityAsync(int cartItemId, int quantityToRemove)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return false;

            if (quantityToRemove <= 0 || item.Quantity <= 0 || item.Quantity < quantityToRemove)
            {
                return false;
            }

            item.Quantity -= quantityToRemove;

            if (item.Quantity <= 0)
            {
                _context.CartItems.Remove(item);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(string email)
        {
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.Email == email);
            if (cart == null) return false;

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
