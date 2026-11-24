using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.Errors;
using VTInformatica.Interfaces;

namespace VTInformatica.Controllers
{
    // Le eccezioni le gestisce GlobalExceptionHandler.
    [Route("[controller]")]
    [ApiController]
    public class ManufacturerController : ControllerBase
    {
        private readonly IManufacturerService _manufacturerService;

        public ManufacturerController(IManufacturerService manufacturerService)
        {
            _manufacturerService = manufacturerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Il service restituisce sempre { message, manufacturers } (lista eventualmente vuota).
            return Ok(await _manufacturerService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var manufacturer = await _manufacturerService.GetByIdAsync(id);
            if (manufacturer == null)
                return NotFoundProblem();
            return Ok(manufacturer);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Create([FromBody] CreateManufacturerDto dto)
        {
            var created = await _manufacturerService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.ManufacturerId }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Update(int id, [FromBody] ManufacturerDto dto)
        {
            var success = await _manufacturerService.UpdateAsync(id, dto);
            return success ? NoContent() : NotFoundProblem();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Delete(int id)
        {
            // Prodotti collegati (anche eliminati): 409 resource_in_use dal service.
            var success = await _manufacturerService.DeleteAsync(id);
            return success ? NoContent() : NotFoundProblem();
        }

        private ObjectResult NotFoundProblem() =>
            this.ApiProblem(StatusCodes.Status404NotFound, "not_found", "Produttore non trovato.");
    }
}
