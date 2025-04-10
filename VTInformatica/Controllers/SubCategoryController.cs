using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Interfaces;

namespace VTInformatica.Controllers
{
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
            try
            {
                var subCategories = await _subCategoryService.GetAllAsync();
                return Ok(subCategories);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during GetAll for SubCategories");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            try
            {
                var subCategory = await _subCategoryService.GetByIdAsync(id);
                return subCategory == null ? NotFound() : Ok(subCategory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during GetSubCategory");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SubCategoryDto dto)
        {
            try
            {
                var newSubCategory = await _subCategoryService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetSubCategory), new { id = newSubCategory.Id }, newSubCategory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Create for SubCategory");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SubCategoryDto dto)
        {
            try
            {
                var success = await _subCategoryService.UpdateAsync(id, dto);
                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Update");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _subCategoryService.DeleteAsync(id);
                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Delete");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
