using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using VTInformatica.DTOs.Order;
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
            try
            {
                var orders = await _orderService.GetAllAsync();
                if (orders == null || !orders.Any())
                    return NotFound("No orders found.");

                return Ok(orders);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving all orders.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                    return NotFound("No orders found.");

                return Ok(order);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving order details.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("user/{email}")]
        [Authorize]
        public async Task<IActionResult> GetOrdersByEmail(string email)
        {
            if (!User.HasEmail(email) && !User.IsStaff())
                return Forbid();

            try
            {
                var orders = await _orderService.GetOrdersByEmailAsync(email);
                if (orders == null || !orders.Any())
                    return NotFound("No orders found.");

                return Ok(orders);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving orders by email.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("from-cart/{userEmail}")]
        [Authorize]
        public async Task<IActionResult> CreateOrderFromCart(string userEmail)
        {
            // Nessuna eccezione per Admin/Seller: si ordina solo dal proprio carrello.
            if (!User.HasEmail(userEmail))
                return Forbid();

            try
            {
                var order = await _orderService.CreateOrderFromCartAsync(userEmail);
                return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating an order from the cart.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}/delete")]
        [Authorize]
        public async Task<IActionResult> SoftDelete(int id)
        {
            try
            {
                // 404 anche se l'ordine esiste ma non è dell'utente: non ne riveliamo l'esistenza.
                if (!await CanManageOrderAsync(id))
                    return NotFound(new { message = "Order not found or already deleted" });

                var success = await _orderService.SoftDeleteAsync(id);
                if (success)
                {
                    return Ok(new { message = "Order correctly deleted." });
                }
                else
                {
                    return NotFound(new { message = "Order not found or already deleted" });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while soft-deleting the order.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}/restore")]
        [Authorize]
        public async Task<IActionResult> RestoreOrder(int id)
        {
            try
            {
                if (!await CanManageOrderAsync(id))
                    return NotFound(new { message = "Order not found or already active" });

                var success = await _orderService.RestoreAsync(id);
                if (success)
                {
                    return Ok(new { message = "Order correctly restored." });
                }
                else
                {
                    return NotFound(new { message = "Order not found or already active" });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while restoring the order.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }


}
