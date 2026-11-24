using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VTInformatica.DTOs.Product;
using VTInformatica.Errors;
using VTInformatica.Interfaces;
using VTInformatica.Mapping;

namespace VTInformatica.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();

            // Catalogo vuoto = 200 []. Le eccezioni le gestisce GlobalExceptionHandler.
            return Ok(products.Select(ProductMapper.ToProductDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return this.ApiProblem(StatusCodes.Status404NotFound, "not_found", "Prodotto non trovato.");

            return Ok(ProductMapper.ToProductDto(product));
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string query)
        {
            // Difensivo: una query vuota o di soli spazi la rifiuta già la validazione (400 validation_failed).
            if (string.IsNullOrWhiteSpace(query))
                return this.ApiProblem(StatusCodes.Status400BadRequest, "invalid_query", "La ricerca non può essere vuota.");

            // Nessun risultato = 200 [].
            var products = await _productService.SearchProductsAsync(query);

            return Ok(products.Select(ProductMapper.ToProductDto));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            // Sottocategoria o produttore inesistenti: 400 dalle eccezioni di dominio.
            var newProduct = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            // Riferimenti inesistenti (400) e stock cambiato (409 stock_conflict): eccezioni di dominio.
            var success = await _productService.UpdateAsync(id, dto);
            return success ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            // Nessun prodotto = 200 [].
            return Ok(await _productService.GetProductsByCategoryIdAsync(categoryId));
        }

        [HttpGet("subcategory/{subCategoryId}")]
        public async Task<IActionResult> GetProductsBySubCategory(int subCategoryId)
        {
            return Ok(await _productService.GetProductsBySubCategoryIdAsync(subCategoryId));
        }

        [HttpGet("manufacturer/{manufacturerId}")]
        public async Task<IActionResult> GetProductsByManufacturer(int manufacturerId)
        {
            return Ok(await _productService.GetProductsByManufacturerIdAsync(manufacturerId));
        }
    }
}
