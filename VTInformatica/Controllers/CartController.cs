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
            return currentEmail != null && await _cartService.ItemBelongsToAsync(cartItemId, currentEmail);
        }

        // Route con identità dal token: l'email non viaggia nell'URL.
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

            // Un utente senza carrello ne ha uno vuoto: la lettura non è mai un 404.
            return Ok(await _cartService.GetCartByEmailAsync(email));
        }

        private async Task<IActionResult> AddItemCore(string? email, CartItemDto dto)
        {
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
