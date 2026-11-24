using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Interfaces;

namespace VTInformatica.Controllers
{
    // Nessun try/catch (AUDIT #11): le eccezioni le gestisce GlobalExceptionHandler.
    // NotFound() senza corpo diventa ProblemDetails not_found (ApiController + ApiProblems.Customize).
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Seller")]
    public class SubCategoryController : ControllerBase
    {
        private readonly ISubCategoryService _subCategoryService;

        public SubCategoryController(ISubCategoryService subCategoryService)
        {
            _subCategoryService = subCategoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var subCategories = await _subCategoryService.GetAllAsync();
            return Ok(subCategories);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            var subCategory = await _subCategoryService.GetByIdAsync(id);
            return subCategory == null ? NotFound() : Ok(subCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubCategoryDto dto)
        {
            var newSubCategory = await _subCategoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetSubCategory), new { id = newSubCategory.Id }, newSubCategory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SubCategoryDto dto)
        {
            var success = await _subCategoryService.UpdateAsync(id, dto);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Prodotti collegati (anche eliminati): 409 resource_in_use dal service.
            var success = await _subCategoryService.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
