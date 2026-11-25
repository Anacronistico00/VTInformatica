using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VTInformatica.DTOs;
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
        public async Task<IActionResult> GetAll([FromQuery] PageQuery paging)
        {
            // Senza page/pageSize: lista completa. Con uno dei due: stessa lista paginata + X-Total-Count.
            // Lista vuota = 200 []. Le eccezioni le gestisce GlobalExceptionHandler.
            if (!paging.IsRequested)
                return Ok(await _orderService.GetAllAsync());

            var (orders, total) = await _orderService.GetPageAsync(paging);
            Response.Headers[PageQuery.TotalCountHeader] = total.ToString();
            return Ok(orders);
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

        // Route con identità dal token: l'email non viaggia nell'URL.
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
        public async Task<IActionResult> CreateMyOrderFromCart([FromBody] CreateOrderFromCartDto? dto = null)
        {
            var email = User.GetEmail();
            if (string.IsNullOrEmpty(email))
                return this.ApiProblem(StatusCodes.Status401Unauthorized, "unauthorized", "Utente non autenticato.");

            return await CreateOrderCore(email, dto);
        }

        private async Task<IActionResult> CreateOrderCore(string email, CreateOrderFromCartDto? dto)
        {
            // Carrello vuoto (409), prodotto inesistente (404) e stock insufficiente (409) sono
            // eccezioni di dominio: le traduce GlobalExceptionHandler.
            var order = await _orderService.CreateOrderFromCartAsync(email, dto?.CustomerComment);
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
            // Stock insufficiente: 409 insufficient_stock (InsufficientStockException, GlobalExceptionHandler).
            if (!await CanManageOrderAsync(id) || !await _orderService.RestoreAsync(id))
                return this.ApiProblem(StatusCodes.Status404NotFound, "not_found", "Ordine non trovato o già attivo.");

            return Ok(new { message = "Order correctly restored." });
        }
    }


}
