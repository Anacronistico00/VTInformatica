using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Runtime.Intrinsics.X86;
using VTInformatica.Data;
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
            var subCategory = await GetReferencedSubCategoryAsync(dto.SubCategoryId, dto.ManufacturerId);

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
            var subCategory = await GetReferencedSubCategoryAsync(dto.SubCategoryId, dto.ManufacturerId);

            var product = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            product.ManufacturerId = dto.ManufacturerId;
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.FullDescription = dto.FullDescription;
            product.Price = dto.Price;
            if (dto.ExpectedQuantity == null)
            {
                // Comportamento precedente (sovrascrive lo stock): resta finché il FE non invia ExpectedQuantity.
                product.Quantity = dto.Quantity;
            }
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

            // Il prodotto esce dai carrelli e il totale persistito viene ricalcolato.
            // Un solo SaveChanges: flag, righe e totali nella stessa transazione.
            var carts = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .Where(c => c.Items.Any(i => i.ProductId == id))
                .ToListAsync();

            foreach (var cart in carts)
            {
                var removed = cart.Items.Where(i => i.ProductId == id).ToList();
                _context.CartItems.RemoveRange(removed);
                cart.TotalPrice = cart.Items.Except(removed)
                    .Where(i => i.Product != null)
                    .Sum(i => i.Quantity * i.Product.Price);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<GetProductDto>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var products = await _context.Products.Include(p => p.Manufacturer).Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.SubCategory)
            .Where(p => p.CategoryId == categoryId).ToListAsync();

            return products.Select(ProductMapper.ToGetProductDto).ToList();
        }

        public async Task<List<GetProductDto>> GetProductsBySubCategoryIdAsync(int subCategoryId)
        {
            var products = await _context.Products.Include(p => p.Manufacturer).Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.SubCategory)
                .Where(p => p.SubCategoryId == subCategoryId).ToListAsync();

            return products.Select(ProductMapper.ToGetProductDto).ToList();
        }

        public async Task<List<GetProductDto>> GetProductsByManufacturerIdAsync(int manufacturerId)
        {
            var products = await _context.Products.Include(p => p.Manufacturer).Include(p => p.ProductImages).Include(p => p.Category).Include(p => p.SubCategory)
               .Where(p => p.ManufacturerId == manufacturerId).ToListAsync();

            return products.Select(ProductMapper.ToGetProductDto).ToList();
        }
    }
}

