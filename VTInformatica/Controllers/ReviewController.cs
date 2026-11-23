using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using VTInformatica.DTOs.Review;
using VTInformatica.Extensions;
using VTInformatica.Interfaces;
using VTInformatica.Models;
using VTInformatica.Services;

namespace VTInformatica.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _service;

        public ReviewController(IReviewService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var reviews = await _service.GetAllAsync();
                return Ok(new GetReviewsResponseDto
                {
                    Message = "All reviews correctly get",
                    Reviews = reviews
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching all reviews");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var review = await _service.GetByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }
                return Ok(new GetReviewByIdResponseDto
                {
                    Message = "All reviews correctly get",
                    Review = review
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching review");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            // Identità sempre dal JWT: UserId/Email del body vengono sovrascritti.
            var loggedUserId = User.GetUserId();
            var loggedEmail = User.GetEmail();
            if (string.IsNullOrEmpty(loggedUserId) || string.IsNullOrEmpty(loggedEmail))
            {
                return Unauthorized(new { message = "User not logged in." });
            }

            dto.UserId = loggedUserId;
            dto.Email = loggedEmail;

            try
            {
                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.ReviewId }, created);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating review");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto dto)
        {
            try
            {
                var loggedUserId = User.GetUserId();
                if (string.IsNullOrEmpty(loggedUserId))
                {
                    return Unauthorized(new { message = "User not logged in." });
                }

                var success = await _service.UpdateAsync(id, dto, loggedUserId);
                return success ? NoContent() : NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning(ex, "Unauthorized update attempt");
                return Forbid();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating review");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var loggedUserId = User.GetUserId();
                if (string.IsNullOrEmpty(loggedUserId))
                {
                    return Unauthorized(new { message = "User not logged in." });
                }

                var success = await _service.DeleteAsync(id, loggedUserId);
                return success ? Ok(new { message = "Comment correctly deleted!"}) : NotFound(new { message = "Review not found." });
            }
            catch (UnauthorizedAccessException ex)
            {
                // 404 e non 403: non riveliamo l'esistenza della review di un altro utente.
                Log.Warning(ex, "Unauthorized delete attempt");
                return NotFound(new { message = "Review not found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting review");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
