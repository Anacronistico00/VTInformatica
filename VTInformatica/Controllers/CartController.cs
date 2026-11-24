using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VTInformatica.DTOs.Cart;
using VTInformatica.Errors;
using VTInformatica.Extensions;
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

        // L'item deve appartenere al carrello dell'utente autenticato (identità dai claim del JWT).
        private async Task<bool> ItemBelongsToCurrentUserAsync(int cartItemId)
        {
            var currentEmail = User.GetEmail();
            if (currentEmail == null)
                return false;

            var cart = await _cartService.GetCartByEmailAsync(currentEmail);
            return cart != null && cart.Items.Any(i => i.Id == cartItemId);
        }

        [HttpGet("{email}")]
        public async Task<IActionResult> GetCart(string email)
        {
            if (!User.HasEmail(email))
                return Forbid();

            var cart = await _cartService.GetCartByEmailAsync(email);
            if (cart == null)
                return this.ApiProblem(StatusCodes.Status404NotFound, "not_found", "Carrello non trovato.");

            return Ok(cart);
        }

        [HttpPost("{email}/add")]
        public async Task<IActionResult> AddItem(string email, [FromBody] CartItemDto dto)
        {
            if (!User.HasEmail(email))
                return Forbid();

            // Controllo ridondante (fuori scope M2 n. 9, M3d): resta, nel formato unico.
            var userId = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userId))
                return this.ApiProblem(StatusCodes.Status401Unauthorized, "unauthorized", "Utente non autenticato.");

            // Prodotto inesistente (404) e stock insufficiente (409): eccezioni di dominio,
            // le traduce GlobalExceptionHandler.
            await _cartService.AddItemAsync(email, dto);
            return Ok(new { Message = "Item Added succesfully!" });
        }

        [HttpDelete("{itemId}/delete")]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            // 404 anche se l'item esiste ma è di un altro utente: non ne riveliamo l'esistenza.
            // NotFound() senza corpo diventa ProblemDetails not_found (ApiController + ApiProblems.Customize).
            if (!await ItemBelongsToCurrentUserAsync(itemId))
                return NotFound();

            var success = await _cartService.RemoveItemAsync(itemId);
            return success ? NoContent() : NotFound();
        }

        [HttpPatch("{cartItemId}/delete/byquantity")]
        public async Task<IActionResult> RemoveItemQuantity(int cartItemId, [FromQuery] int quantityToRemove)
        {
            if (quantityToRemove <= 0)
                return this.ApiProblem(StatusCodes.Status400BadRequest, "invalid_quantity", "La quantità da rimuovere deve essere maggiore di zero.");

            if (!await ItemBelongsToCurrentUserAsync(cartItemId)
                || !await _cartService.RemoveItemQuantityAsync(cartItemId, quantityToRemove))
                return this.ApiProblem(StatusCodes.Status404NotFound, "not_found", "Articolo non trovato o quantità da rimuovere non valida.");

            return Ok(new { message = "Quantità dell'articolo aggiornata con successo." });
        }

        [HttpDelete("{email}/clear")]
        public async Task<IActionResult> ClearCart(string email)
        {
            if (!User.HasEmail(email))
                return Forbid();

            var success = await _cartService.ClearCartAsync(email);
            return success ? NoContent() : NotFound();
        }
    }

}
