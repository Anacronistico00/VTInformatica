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

        // Route con identità dal token (M3c1): l'email non viaggia mai nell'URL.
        [HttpGet("me")]
        public Task<IActionResult> GetMyCart() => GetCartCore(User.GetEmail());

        [HttpPost("me/add")]
        public Task<IActionResult> AddItemToMyCart([FromBody] CartItemDto dto) => AddItemCore(User.GetEmail(), dto);

        [HttpDelete("me/clear")]
        public Task<IActionResult> ClearMyCart() => ClearCartCore(User.GetEmail());

        [Obsolete("Usare GET /Cart/me: l'identità arriva dal token.")]
        [HttpGet("{email}")]
        public async Task<IActionResult> GetCart(string email)
        {
            if (!User.HasEmail(email))
                return Forbid();

            return await GetCartCore(email);
        }

        [Obsolete("Usare POST /Cart/me/add: l'identità arriva dal token.")]
        [HttpPost("{email}/add")]
        public async Task<IActionResult> AddItem(string email, [FromBody] CartItemDto dto)
        {
            if (!User.HasEmail(email))
                return Forbid();

            return await AddItemCore(email, dto);
        }

        private async Task<IActionResult> GetCartCore(string? email)
        {
            if (string.IsNullOrEmpty(email))
                return this.ApiProblem(StatusCodes.Status401Unauthorized, "unauthorized", "Utente non autenticato.");

            var cart = await _cartService.GetCartByEmailAsync(email);
            if (cart == null)
                return this.ApiProblem(StatusCodes.Status404NotFound, "not_found", "Carrello non trovato.");

            return Ok(cart);
        }

        private async Task<IActionResult> AddItemCore(string? email, CartItemDto dto)
        {
            // Controllo ridondante (fuori scope M2 n. 9, M3d): resta, nel formato unico.
            if (string.IsNullOrEmpty(email))
                return this.ApiProblem(StatusCodes.Status401Unauthorized, "unauthorized", "Utente non autenticato.");

            // Prodotto inesistente (404) e stock insufficiente (409): eccezioni di dominio,
            // le traduce GlobalExceptionHandler.
            await _cartService.AddItemAsync(email, dto);
            return Ok(new { Message = "Item Added succesfully!" });
        }

        private async Task<IActionResult> ClearCartCore(string? email)
        {
            if (string.IsNullOrEmpty(email))
                return this.ApiProblem(StatusCodes.Status401Unauthorized, "unauthorized", "Utente non autenticato.");

            var success = await _cartService.ClearCartAsync(email);
            return success ? NoContent() : NotFound();
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

        [Obsolete("Usare DELETE /Cart/me/clear: l'identità arriva dal token.")]
        [HttpDelete("{email}/clear")]
        public async Task<IActionResult> ClearCart(string email)
        {
            if (!User.HasEmail(email))
                return Forbid();

            return await ClearCartCore(email);
        }
    }

}
