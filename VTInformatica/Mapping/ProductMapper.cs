using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.DTOs.Product;
using VTInformatica.DTOs.Review;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Models;

namespace VTInformatica.Mapping
{
    /// <summary>
    /// Unico punto di mapping Product → DTO (prima copiato 5 volte tra ProductController e ProductService).
    /// </summary>
    public static class ProductMapper
    {
        // Risposta di GET /Product, /Product/{id} e /Product/search.
        public static ProductDto ToProductDto(Product p) => new ProductDto
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
            // Categoria derivata dalla sottocategoria.
            Category = p.SubCategory?.Category != null ? new CategoryDto()
            {
                Id = p.SubCategory.Category.Id,
                Name = p.SubCategory.Category.Name,
            } : null,
            SubCategory = ToSubCategoryDto(p.SubCategory),
            ProductImages = ToImageDtos(p),
            Reviews = p.Reviews != null
                ? p.Reviews.Select(r => new ProductReviewDto
                {
                    ReviewId = r.ReviewId,
                    ReviewComment = r.ReviewComment,
                    ReviewRating = r.ReviewRating,
                    CreatedAt = r.CreatedAt
                }).ToList()
                : new List<ProductReviewDto>()
        };

        // Risposta di /Product/category/{id}, /Product/subcategory/{id}, /Product/manufacturer/{id}.
        public static GetProductDto ToGetProductDto(Product p) => new GetProductDto
        {
            ManufacturerId = p.ManufacturerId,
            Manufacturer = p.Manufacturer != null ? new CreateManufacturerDto()
            {
                ManufacturerName = p.Manufacturer.ManufacturerName,
                ManufacturerLogo = p.Manufacturer.ManufacturerLogo,
            } : null,
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            FullDescription = p.FullDescription,
            Price = p.Price,
            Quantity = p.Quantity,
            Category = p.SubCategory?.Category != null ? new GetCategoryDto
            {
                Id = p.SubCategory.Category.Id,
                Name = p.SubCategory.Category.Name,
            } : null,
            SubCategory = ToSubCategoryDto(p.SubCategory),
            ProductImages = ToImageDtos(p)
        };

        private static SubCategoryDto? ToSubCategoryDto(SubCategory? sc) => sc != null ? new SubCategoryDto
        {
            Id = sc.Id,
            Name = sc.Name,
            CategoryId = sc.CategoryId,
        } : null;

        private static List<ProductImageDto> ToImageDtos(Product p) =>
            p.ProductImages?.Select(img => new ProductImageDto { ImageUrl = img.ImageUrl }).ToList()
            ?? new List<ProductImageDto>();
    }
}
