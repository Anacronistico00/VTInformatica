using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;
using VTInformatica.Data;
using VTInformatica.DTOs.Account;
using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.DTOs.Product;
using VTInformatica.DTOs.Review;
using VTInformatica.DTOs.SubCategory;
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
            try
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

                if (products == null || !products.Any())
                    return NotFound(new { message = "No products found." });

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during GetAll");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new { message = "No product found." });

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
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting Product");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            try
            {
                var newProduct = await _productService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, dto);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Create");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            try
            {
                var success = await _productService.UpdateAsync(id, dto);
                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Update");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _productService.DeleteAsync(id);
                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during Delete");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryIdAsync(categoryId);
                if (products == null || !products.Any())
                    return NotFound(new { message = "No products found for this category." });

                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting products category");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("subcategory/{subCategoryId}")]
        public async Task<IActionResult> GetProductsBySubCategory(int subCategoryId)
        {
            try
            {
                var products = await _productService.GetProductsBySubCategoryIdAsync(subCategoryId);
                if (products == null || !products.Any())
                    return NotFound(new { message = "No products found for this subcategory." });

                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting products subcategory");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("manufacturer/{manufacturerId}")]
        public async Task<IActionResult> GetProductsByManufacturer(int manufacturerId)
        {
            try
            {
                var products = await _productService.GetProductsByManufacturerIdAsync(manufacturerId);
                if (products == null || !products.Any())
                    return NotFound(new { message = "No products found for this manufacturer." });

                return Ok(products);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting products for manufacturer");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
