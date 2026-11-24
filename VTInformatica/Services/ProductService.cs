using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Runtime.Intrinsics.X86;
using VTInformatica.Data;
using VTInformatica.DTOs;
using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.DTOs.Product;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Exceptions;
using VTInformatica.Interfaces;
using VTInformatica.Mapping;
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
            return await ProductsWithDetails().ToListAsync();
        }

        public async Task<(List<Product> Items, int Total)> GetPageAsync(PageQuery paging)
        {
            var total = await _context.Products.CountAsync();
            var items = await ProductsWithDetails()
                .OrderBy(p => p.Id)
                .Skip(paging.Skip)
                .Take(paging.Take)
                .ToListAsync();

            return (items, total);
        }

        // Letture senza tracking, con solo ciò che i DTO mostrano.
        private IQueryable<Product> ProductsWithDetails() => _context.Products.AsNoTracking()
            .Include(p => p.Manufacturer)
            .Include(p => p.ProductImages)
            .Include(p => p.SubCategory!.Category)
            .Include(p => p.Reviews);

        public async Task<Product?> GetByIdAsync(int id)
        {
            var product = await ProductsWithDetails().FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return null;

            return product;
        }

        public async Task<List<Product>> SearchProductsAsync(string query)
        {
            return await ProductsWithDetails()
                .Where(p =>
                    p.Name.ToLower().Contains(query.ToLower()) ||
                    p.Description.ToLower().Contains(query.ToLower()) ||
                    (p.FullDescription != null && p.FullDescription.ToLower().Contains(query.ToLower()))
                )
                .ToListAsync();
        }

        /// <summary>
        /// Riferimenti del corpo di create/update: sottocategoria e produttore devono esistere (400),
        /// altrimenti la FK fallirebbe al salvataggio con un 500.
        /// </summary>
        private async Task<SubCategory> GetReferencedSubCategoryAsync(int subCategoryId, int manufacturerId)
        {
            var subCategory = await _context.SubCategories.FirstOrDefaultAsync(sc => sc.Id == subCategoryId)
                ?? throw new SubCategoryNotFoundException(subCategoryId);

            if (!await _context.Manufacturers.AnyAsync(m => m.ManufacturerId == manufacturerId))
                throw new ManufacturerNotFoundException(manufacturerId);

            return subCategory;
        }

        public async Task<Product> CreateAsync(CreateProductDto dto)
        {
            await GetReferencedSubCategoryAsync(dto.SubCategoryId, dto.ManufacturerId);

            var product = new Product ()
            {
                Name = dto.Name.Trim(),
                ManufacturerId = dto.ManufacturerId,
                Description = dto.Description.Trim(),
                Price = dto.Price,
                Quantity = dto.Quantity,
                FullDescription = dto.FullDescription?.Trim(),
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
            await GetReferencedSubCategoryAsync(dto.SubCategoryId, dto.ManufacturerId);

            var product = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            product.ManufacturerId = dto.ManufacturerId;
            product.Name = dto.Name.Trim();
            product.Description = dto.Description.Trim();
            product.FullDescription = dto.FullDescription?.Trim();
            product.Price = dto.Price;
            if (dto.ExpectedQuantity == null)
            {
                // Senza ExpectedQuantity la quantità viene sovrascritta (compatibilità con i client che non la inviano).
                product.Quantity = dto.Quantity;
            }
            product.SubCategoryId = dto.SubCategoryId;

            // Confronta gli url esistenti con quelli nuovi, rimuove tutti gli url e aggiunge quelli nuovi
            if (product.ProductImages == null || dto.ProductImages == null || !dto.ProductImages.Any())
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

            if (dto.ExpectedQuantity == null)
            {
                await _context.SaveChangesAsync();
                return true;
            }

            // Lost update: stock scritto solo se è ancora quello visto dall'admin (UPDATE ... WHERE Quantity = expected).
            // Tutto o niente: se lo stock è cambiato il dispose della transazione annulla anche gli altri campi.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var updated = await _context.Products
                .Where(p => p.Id == id && p.Quantity == dto.ExpectedQuantity)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Quantity, dto.Quantity));
            if (updated == 0)
            {
                throw new StockConflictException(id);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Il query filter esclude i prodotti già eliminati: un secondo delete risponde 404.
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            product.IsDeleted = true;

            // Il prodotto esce dai carrelli. Un solo SaveChanges: flag e righe nella stessa transazione.
            _context.CartItems.RemoveRange(await _context.CartItems.Where(i => i.ProductId == id).ToListAsync());

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<GetProductDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            // La categoria di un prodotto è quella della sua sottocategoria.
            var products = await _context.Products.AsNoTracking().Include(p => p.Manufacturer).Include(p => p.ProductImages).Include(p => p.SubCategory!.Category)
                .Where(p => p.SubCategory!.CategoryId == categoryId).ToListAsync();

            return products.Select(ProductMapper.ToGetProductDto).ToList();
        }

        public async Task<List<GetProductDto>> GetProductsBySubCategoryIdAsync(int subCategoryId)
        {
            var products = await _context.Products.AsNoTracking().Include(p => p.Manufacturer).Include(p => p.ProductImages).Include(p => p.SubCategory!.Category)
                .Where(p => p.SubCategoryId == subCategoryId).ToListAsync();

            return products.Select(ProductMapper.ToGetProductDto).ToList();
        }

        public async Task<List<GetProductDto>> GetProductsByManufacturerIdAsync(int manufacturerId)
        {
            var products = await _context.Products.AsNoTracking().Include(p => p.Manufacturer).Include(p => p.ProductImages).Include(p => p.SubCategory!.Category)
               .Where(p => p.ManufacturerId == manufacturerId).ToListAsync();

            return products.Select(ProductMapper.ToGetProductDto).ToList();
        }
    }
}

