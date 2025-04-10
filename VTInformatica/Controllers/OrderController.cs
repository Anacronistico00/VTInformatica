using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
                return order is null ? NotFound() : Ok(order);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving order details.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetOrdersByUser(string userId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving orders by user ID.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("from-cart/{userId}")]
        [Authorize]
        public async Task<IActionResult> CreateOrderFromCart(string userId)
        {
            try
            {
                var order = await _orderService.CreateOrderFromCartAsync(userId);
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
                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while soft-deleting the order.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }


}
