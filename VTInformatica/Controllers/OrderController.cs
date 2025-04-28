using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using VTInformatica.DTOs.Order;
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
