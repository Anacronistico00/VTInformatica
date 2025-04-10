using Microsoft.AspNetCore.Mvc;
using VTInformatica.DTOs.Cart;

namespace VTInformatica.Interfaces
{
    public interface ICartService
    {
        Task<CartDto?> GetCartByUserIdAsync(string userId);
        Task<CartDto> AddItemAsync(string userId, CartItemDto itemDto);
        Task<bool> RemoveItemAsync(int cartItemId);
        Task<bool> RemoveItemQuantityAsync(int cartItemId, int quantityToRemove);
        Task<bool> ClearCartAsync(string userId);
    }
}
