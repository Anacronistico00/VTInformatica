using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
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

        [HttpGet("{email}")]
        public async Task<IActionResult> GetCart(string email)
        {
            try
            {
                var cart = await _cartService.GetCartByEmailAsync(email);
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

        [HttpPost("{email}/add")]
        public async Task<IActionResult> AddItem(string email, [FromBody] CartItemDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var updatedCart = await _cartService.AddItemAsync(email, dto);
                return Ok(new { Message = "Item Added succesfully!"});
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during add item to cart");
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
                Log.Error(ex, "Error removing item from cart");
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

        [HttpDelete("{email}/clear")]
        public async Task<IActionResult> ClearCart(string email)
        {
            try
            {
                var success = await _cartService.ClearCartAsync(email);
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
