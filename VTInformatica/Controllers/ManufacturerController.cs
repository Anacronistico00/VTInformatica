using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.Interfaces;

namespace VTInformatica.Controllers
{
    [Route("api/[controller]")]
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
            try
            {
                var response = await _manufacturerService.GetAllAsync();
                if (response == null)
                {
                    return NotFound(new { message = "Manufacturers not found." });
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching all manufacturers");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var manufacturer = await _manufacturerService.GetByIdAsync(id);
                if (manufacturer == null)
                    return NotFound(new { message = "Manufacturer not found." });
                return Ok(manufacturer);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching manufacturer");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Create([FromBody] CreateManufacturerDto dto)
        {
            try
            {
                var created = await _manufacturerService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.ManufacturerId }, created);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating manufacturer");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Update(int id, [FromBody] ManufacturerDto dto)
        {
            try
            {
                var success = await _manufacturerService.UpdateAsync(id, dto);
                return success ? NoContent() : NotFound(new { message = "Manufacturer not found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating manufacturer");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _manufacturerService.DeleteAsync(id);
                return success ? NoContent() : NotFound(new { message = "Manufacturer not found." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting manufacturer");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
