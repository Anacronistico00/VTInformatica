using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VTInformatica.DTOs.Order;
using VTInformatica.Errors;
using VTInformatica.Extensions;
using VTInformatica.Interfaces;
using VTInformatica.Models;
using VTInformatica.Services;

namespace VTInformatica.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Proprietario dell'ordine oppure Admin/Seller (identità dai claim del JWT).
        private async Task<bool> CanManageOrderAsync(int orderId)
        {
            var order = await _orderService.GetByIdAsync(orderId);
            return order != null && (User.IsStaff() || User.HasEmail(order.CustomerEmail));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> GetAll()
        {
            // Lista vuota = 200 []. Le eccezioni le gestisce GlobalExceptionHandler.
            return Ok(await _orderService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return this.ApiProblem(StatusCodes.Status404NotFound, "not_found", "Ordine non trovato.");

            return Ok(order);
        }

        // Route con identità dal token (M3c1): l'email non viaggia mai nell'URL.
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders()
        {
            var email = User.GetEmail();
            if (string.IsNullOrEmpty(email))
                return this.ApiProblem(StatusCodes.Status401Unauthorized, "unauthorized", "Utente non autenticato.");

            return Ok(await _orderService.GetOrdersByEmailAsync(email));
        }

        [HttpPost("me/from-cart")]
        [Authorize]
        public async Task<IActionResult> CreateMyOrderFromCart()
        {
            var email = User.GetEmail();
            if (string.IsNullOrEmpty(email))
                return this.ApiProblem(StatusCodes.Status401Unauthorized, "unauthorized", "Utente non autenticato.");

            return await CreateOrderCore(email);
        }

        [Obsolete("Usare GET /Order/me: l'identità arriva dal token.")]
        [HttpGet("user/{email}")]
        [Authorize]
        public async Task<IActionResult> GetOrdersByEmail(string email)
        {
            if (!User.HasEmail(email) && !User.IsStaff())
                return Forbid();

            return Ok(await _orderService.GetOrdersByEmailAsync(email));
        }

        [Obsolete("Usare POST /Order/me/from-cart: l'identità arriva dal token.")]
        [HttpPost("from-cart/{userEmail}")]
        [Authorize]
        public async Task<IActionResult> CreateOrderFromCart(string userEmail)
        {
            // Nessuna eccezione per Admin/Seller: si ordina solo dal proprio carrello.
            if (!User.HasEmail(userEmail))
                return Forbid();

            return await CreateOrderCore(userEmail);
        }

        private async Task<IActionResult> CreateOrderCore(string email)
        {
            // Carrello vuoto (409), prodotto inesistente (404) e stock insufficiente (409) sono
            // eccezioni di dominio: le traduce GlobalExceptionHandler.
            var order = await _orderService.CreateOrderFromCartAsync(email);
            return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
        }

        [HttpPut("{id}/delete")]
        [Authorize]
        public async Task<IActionResult> SoftDelete(int id)
        {
            // 404 anche se l'ordine esiste ma non è dell'utente: non ne riveliamo l'esistenza.
            if (!await CanManageOrderAsync(id) || !await _orderService.SoftDeleteAsync(id))
                return this.ApiProblem(StatusCodes.Status404NotFound, "not_found", "Ordine non trovato o già annullato.");

            return Ok(new { message = "Order correctly deleted." });
        }

        [HttpPut("{id}/restore")]
        [Authorize]
        public async Task<IActionResult> RestoreOrder(int id)
        {
            // Restore con stock insufficiente risponde ancora 404 (fuori scope M2 n. 3, previsto in M3c).
            if (!await CanManageOrderAsync(id) || !await _orderService.RestoreAsync(id))
                return this.ApiProblem(StatusCodes.Status404NotFound, "not_found", "Ordine non trovato o già attivo.");

            return Ok(new { message = "Order correctly restored." });
        }
    }


}
