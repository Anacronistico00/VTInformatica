using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using VTInformatica.DTOs.Cart;
using VTInformatica.Interfaces;

namespace VTInformatica.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            try
            {
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    return NotFound(new { message = "Cart not found." });
                }
                return Ok(cart);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting user cart");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("{userId}/add")]
        public async Task<IActionResult> AddItem(string userId, [FromBody] CartItemDto dto)
        {
            try
            {
                var updatedCart = await _cartService.AddItemAsync(userId, dto);
                return Ok(updatedCart);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Errore during add item to cart");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{itemId}/delete")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            try
            {
                var success = await _cartService.RemoveItemAsync(itemId);
                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Errore removing item from cart");
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPatch("{cartItemId}/delete/byquantity")]
        public async Task<IActionResult> RemoveItemQuantity(int cartItemId, [FromQuery] int quantityToRemove)
        {
            if (quantityToRemove <= 0)
            {
                return BadRequest(new { message = "La quantità da rimuovere deve essere maggiore di zero." });
            }

            var success = await _cartService.RemoveItemQuantityAsync(cartItemId, quantityToRemove);

            if (success)
            {
                return Ok(new { message = "Quantità dell'articolo aggiornata con successo." });
            }
            else
            {
                return NotFound(new { message = "Articolo non trovato o quantità da rimuovere non valida." });
            }
        }

        [HttpDelete("{userId}/clear")]
        public async Task<IActionResult> ClearCart(string userId)
        {
            try
            {
                var success = await _cartService.ClearCartAsync(userId);
                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error clearing cart");
                return StatusCode(500, "Internal server error.");
            }
        }
    }

}
