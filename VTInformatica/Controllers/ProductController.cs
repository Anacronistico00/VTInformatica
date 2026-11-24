using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VTInformatica.Data;
using VTInformatica.DTOs.Account;
using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.DTOs.Product;
using VTInformatica.DTOs.Review;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Errors;
using VTInformatica.Interfaces;
using VTInformatica.Models;
using VTInformatica.Services;

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
            {
                var products = await _productService.GetAllAsync();

                var dtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    ManufacturerId = p.ManufacturerId,
                    Manufacturer = p.Manufacturer != null ? new ManufacturerDto()
                    {
                        ManufacturerName = p.Manufacturer.ManufacturerName,
                        ManufacturerLogo = p.Manufacturer.ManufacturerLogo,
                    } : null,
                    Name = p.Name,
                    Description = p.Description,
                    FullDescription = p.FullDescription,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Category = p.Category != null ? new CategoryDto()
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name,
                    } : null,
                    SubCategory = p.SubCategory != null ? new SubCategoryDto()
                    {
                        Id = p.SubCategory.Id,
                        Name = p.SubCategory.Name,
                        CategoryId = p.SubCategory.CategoryId,
                    } : null,
                    ProductImages = p.ProductImages != null
                    ? p.ProductImages.Select(img => new ProductImageDto
                    {
                        ImageUrl = img.ImageUrl
                    }).ToList()
                    : new List<ProductImageDto>(),
                    Reviews = p.Reviews != null
                    ? p.Reviews.Select(r => new GetReviewDto
                    {
                        ReviewId = r.ReviewId,
                        ReviewComment = r.ReviewComment,
                        ReviewRating = r.ReviewRating,
                        CreatedAt = r.CreatedAt,
                        CustomerEmail = r.Email
                    }).ToList()
                    : new List<GetReviewDto>()
                });

                // Catalogo vuoto = 200 []. Le eccezioni le gestisce GlobalExceptionHandler.
                return Ok(dtos);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return this.ApiProblem(StatusCodes.Status404NotFound, "not_found", "Prodotto non trovato.");

                var dto = new ProductDto
                {
                    Id = product.Id,
                    ManufacturerId = product.ManufacturerId,
                    Manufacturer = product.Manufacturer != null ? new ManufacturerDto()
                    {
                        ManufacturerName = product.Manufacturer.ManufacturerName,
                        ManufacturerLogo = product.Manufacturer.ManufacturerLogo,
                    } : null,
                    Name = product.Name,
                    Description = product.Description,
                    FullDescription = product.FullDescription,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    Category = product.Category != null ? new CategoryDto()
                    {
                        Id = product.Category.Id,
                        Name = product.Category.Name,
                    } : null,
                    SubCategory = product.SubCategory != null ? new SubCategoryDto()
                    {
                        Id = product.SubCategory.Id,
                        Name = product.SubCategory.Name,
                        CategoryId = product.SubCategory.CategoryId,
                    } : null,
                    ProductImages = product.ProductImages != null
                    ? product.ProductImages.Select(img => new ProductImageDto
                    { 
                        ImageUrl = img.ImageUrl
                    }).ToList()
                    : new List<ProductImageDto>(),
                    Reviews = product.Reviews != null
                    ? product.Reviews.Select(r => new GetReviewDto
                    {
                        ReviewId = r.ReviewId,
                        ReviewComment = r.ReviewComment,
                        ReviewRating = r.ReviewRating,
                        CreatedAt = r.CreatedAt,
                        CustomerEmail = r.Email
                    }).ToList()
                    : new List<GetReviewDto>()
                };

                return Ok(dto);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string query)
        {
            {
                // Difensivo: una query vuota o di soli spazi la rifiuta già la validazione (400 validation_failed).
                if (string.IsNullOrWhiteSpace(query))
                    return this.ApiProblem(StatusCodes.Status400BadRequest, "invalid_query", "La ricerca non può essere vuota.");

                // Nessun risultato = 200 [].
                var products = await _productService.SearchProductsAsync(query);

                var dtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    ManufacturerId = p.ManufacturerId,
                    Manufacturer = p.Manufacturer != null ? new ManufacturerDto()
                    {
                        ManufacturerName = p.Manufacturer.ManufacturerName,
                        ManufacturerLogo = p.Manufacturer.ManufacturerLogo,
                    } : null,
                    Name = p.Name,
                    Description = p.Description,
                    FullDescription = p.FullDescription,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Category = p.Category != null ? new CategoryDto()
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name,
                    } : null,
                    SubCategory = p.SubCategory != null ? new SubCategoryDto()
                    {
                        Id = p.SubCategory.Id,
                        Name = p.SubCategory.Name,
                        CategoryId = p.SubCategory.CategoryId,
                    } : null,
                    ProductImages = p.ProductImages?.Select(img => new ProductImageDto
                    {
                        ImageUrl = img.ImageUrl
                    }).ToList() ?? new List<ProductImageDto>(),
                    Reviews = p.Reviews?.Select(r => new GetReviewDto
                    {
                        ReviewId = r.ReviewId,
                        ReviewComment = r.ReviewComment,
                        ReviewRating = r.ReviewRating,
                        CreatedAt = r.CreatedAt,
                        CustomerEmail = r.Email
                    }).ToList() ?? new List<GetReviewDto>()
                });

                return Ok(dtos);
            }
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
