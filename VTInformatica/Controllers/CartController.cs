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

        // Identità sempre dai claim del JWT, mai dalla route (stessi helper di OrderController).
        private string? CurrentEmail => User.FindFirstValue(ClaimTypes.Email);

        private bool IsCurrentUser(string? email) =>
            CurrentEmail != null && string.Equals(email, CurrentEmail, StringComparison.OrdinalIgnoreCase);

        // L'item deve appartenere al carrello dell'utente autenticato.
        private async Task<bool> ItemBelongsToCurrentUserAsync(int cartItemId)
        {
            if (CurrentEmail == null)
                return false;

            var cart = await _cartService.GetCartByEmailAsync(CurrentEmail);
            return cart != null && cart.Items.Any(i => i.Id == cartItemId);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetCart(string email)
        {
            if (!IsCurrentUser(email))
                return Forbid();

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
            if (!IsCurrentUser(email))
                return Forbid();

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
                // 404 anche se l'item esiste ma è di un altro utente: non ne riveliamo l'esistenza.
                if (!await ItemBelongsToCurrentUserAsync(itemId))
                    return NotFound();

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

            if (!await ItemBelongsToCurrentUserAsync(cartItemId))
                return NotFound(new { message = "Articolo non trovato o quantità da rimuovere non valida." });

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
            if (!IsCurrentUser(email))
                return Forbid();

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
