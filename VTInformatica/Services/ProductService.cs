using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Runtime.Intrinsics.X86;
using VTInformatica.Data;
using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.DTOs.Product;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Interfaces;
using VTInformatica.Models;

namespace VTInformatica.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return null;

            return product;
        }

        public async Task<List<Product>> SearchProductsAsync(string query)
        {
            return await _context.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .Where(p =>
                    p.Name.ToLower().Contains(query.ToLower()) ||
                    p.Description.ToLower().Contains(query.ToLower()) ||
                    (p.FullDescription != null && p.FullDescription.ToLower().Contains(query.ToLower()))
                )
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(CreateProductDto dto)
        {
            var subCategory = await _context.SubCategories.FirstOrDefaultAsync(sc => sc.Id == dto.SubCategoryId);

            if (subCategory == null)
                throw new ArgumentException("La sottocategoria specificata non esiste.");

            var product = new Product ()
            {
                Name = dto.Name,
                ManufacturerId = dto.ManufacturerId,
                Description = dto.Description,
                Price = dto.Price,
                Quantity = dto.Quantity,
                FullDescription = dto.FullDescription,
                CategoryId = subCategory.CategoryId,
                SubCategoryId = dto.SubCategoryId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();



            product.ProductImages = dto.ProductImages?.Select(img => new ProductImage
            {
                ImageUrl = img.ImageUrl,
                ProductId = product.Id
            }).ToList();
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
        {
            var subCategory = await _context.SubCategories.FirstOrDefaultAsync(sc => sc.Id == dto.SubCategoryId);

            if (subCategory == null)
                throw new ArgumentException("La sottocategoria specificata non esiste.");

            var product = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            product.ManufacturerId = dto.ManufacturerId;
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.FullDescription = dto.FullDescription;
            product.Price = dto.Price;
            product.Quantity = dto.Quantity;
            product.CategoryId = subCategory.CategoryId;
            product.SubCategoryId = dto.SubCategoryId;

            // Confronta gli url esistenti con quelli nuovi, rimuove tutti gli url e aggiunge quelli nuovi
            if(product.ProductImages == null || !product.ProductImages.Any() && dto.ProductImages == null || !dto.ProductImages.Any())
            {
                return false;
            }
            var existingImageUrls = product.ProductImages.Select(img => img.ImageUrl).ToList();
            foreach (var image in product.ProductImages.ToList())
            {
                if (!dto.ProductImages.Any(newImg => newImg.ImageUrl == image.ImageUrl))
                {
                    _context.ProductImages.Remove(image);
                }
            }
            foreach (var img in dto.ProductImages)
            {
                if (!existingImageUrls.Contains(img.ImageUrl))
                {
                    product.ProductImages.Add(new ProductImage
                    {
                        ImageUrl = img.ImageUrl,
                        ProductId = product.Id
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<GetProductDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var products = await _context.Products.Include(p => p.Manufacturer).Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.SubCategory)
            .Where(p => p.CategoryId == categoryId).ToListAsync();

            if(products != null)
            {
            var result = products.Select(p => new GetProductDto
                {
                    ManufacturerId = p.ManufacturerId,
                    Manufacturer = p.Manufacturer != null ? new CreateManufacturerDto()
                        {
                            ManufacturerName = p.Manufacturer.ManufacturerName,
                            ManufacturerLogo = p.Manufacturer.ManufacturerLogo,
                        } : null,
                    Name = p.Name,
                    Description = p.Description,
                    FullDescription = p.FullDescription,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Category = p.Category != null ? new CategoryDto
                        {
                            Id = p.Category.Id,
                            Name = p.Category.Name,
                        } : null,
                    SubCategory = p.SubCategory != null ? new SubCategoryDto
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
                        : new List<ProductImageDto>()
                }).ToList();
            return result;
            }

            return null;

        }

        public async Task<List<GetProductDto>> GetProductsBySubCategoryIdAsync(int subCategoryId)
        {
            var products = await _context.Products.Include(p => p.Manufacturer).Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.SubCategory)
                .Where(p => p.SubCategoryId == subCategoryId).ToListAsync();

            var result = products.Select(p => new GetProductDto
            {
                ManufacturerId = p.ManufacturerId,
                Manufacturer = p.Manufacturer != null ? new CreateManufacturerDto()
                    {
                        ManufacturerName = p.Manufacturer.ManufacturerName,
                        ManufacturerLogo = p.Manufacturer.ManufacturerLogo,
                    } : null,
                Name = p.Name,
                Description = p.Description,
                FullDescription = p.FullDescription,
                Price = p.Price,
                Quantity = p.Quantity,
                Category = p.Category != null ? new CategoryDto
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name,
                    } : null,
                SubCategory = p.SubCategory != null ? new SubCategoryDto
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
                     : new List<ProductImageDto>()
            }).ToList();

            return result;
        }

        public async Task<List<GetProductDto>> GetProductsByManufacturerIdAsync(int manufacturerId)
        {
            var products = await _context.Products.Include(p => p.Manufacturer).Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.SubCategory)
               .Where(p => p.ManufacturerId == manufacturerId).ToListAsync();

            var result = products.Select(p => new GetProductDto
            {
                ManufacturerId = p.ManufacturerId,
                Manufacturer = p.Manufacturer != null ? new CreateManufacturerDto()
                    {
                        ManufacturerName = p.Manufacturer.ManufacturerName,
                        ManufacturerLogo = p.Manufacturer.ManufacturerLogo,
                    } : null,
                Name = p.Name,
                Description = p.Description,
                FullDescription = p.FullDescription,
                Price = p.Price,
                Quantity = p.Quantity,
                Category = p.Category != null ? new CategoryDto
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name,
                    } : null,
                SubCategory = p.SubCategory != null ? new SubCategoryDto
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
                    : new List<ProductImageDto>()
            }).ToList();

            return result;
        }
    }
}

