using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using VTInformatica.DTOs.Review;
using VTInformatica.Errors;
using VTInformatica.Extensions;
using VTInformatica.Interfaces;

namespace VTInformatica.Controllers
{
    // Nessun catch generico (AUDIT #11): le eccezioni le gestisce GlobalExceptionHandler.
    // Restano solo i catch di UnauthorizedAccessException: sono la politica di M1 e dipendono
    // dall'endpoint (update altrui → 403, delete altrui → 404).
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
            var reviews = await _service.GetAllAsync();
            return Ok(new GetReviewsResponseDto
            {
                Message = "All reviews correctly get",
                Reviews = reviews
            });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> GetById(int id)
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            // Identità sempre dal JWT: UserId/Email del body vengono sovrascritti.
            var loggedUserId = User.GetUserId();
            var loggedEmail = User.GetEmail();
            if (string.IsNullOrEmpty(loggedUserId) || string.IsNullOrEmpty(loggedEmail))
            {
                return NotLoggedIn();
            }

            dto.UserId = loggedUserId;
            dto.Email = loggedEmail;

            // ProductId inesistente: oggi violazione FK → 500 internal_error (validazione prevista in M3c).
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.ReviewId }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto dto)
        {
            var loggedUserId = User.GetUserId();
            if (string.IsNullOrEmpty(loggedUserId))
            {
                return NotLoggedIn();
            }

            try
            {
                var success = await _service.UpdateAsync(id, dto, loggedUserId);
                return success ? NoContent() : NotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning(ex, "Unauthorized update attempt");
                return Forbid();
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var loggedUserId = User.GetUserId();
            if (string.IsNullOrEmpty(loggedUserId))
            {
                return NotLoggedIn();
            }

            try
            {
                var success = await _service.DeleteAsync(id, loggedUserId);
                return success ? Ok(new { message = "Comment correctly deleted!" }) : ReviewNotFound();
            }
            catch (UnauthorizedAccessException ex)
            {
                // 404 e non 403: non riveliamo l'esistenza della review di un altro utente.
                Log.Warning(ex, "Unauthorized delete attempt");
                return ReviewNotFound();
            }
        }

        private ObjectResult ReviewNotFound() =>
            this.ApiProblem(StatusCodes.Status404NotFound, "not_found", "Recensione non trovata.");

        private ObjectResult NotLoggedIn() =>
            this.ApiProblem(StatusCodes.Status401Unauthorized, "unauthorized", "Utente non autenticato.");
    }
}
