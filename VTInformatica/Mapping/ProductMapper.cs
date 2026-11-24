using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.Product;
using VTInformatica.DTOs.Review;
using VTInformatica.Models;

namespace VTInformatica.Mapping
{
    /// <summary>Unico punto di mapping Product → DTO.</summary>
    public static class ProductMapper
    {
        // Risposta di GET /Product, /Product/{id} e /Product/search.
        public static ProductDto ToProductDto(Product p) => new ProductDto
        {
            Id = p.Id,
            ManufacturerId = p.ManufacturerId,
            Manufacturer = p.Manufacturer != null ? ManufacturerMapper.ToDto(p.Manufacturer) : null,
            Name = p.Name,
            Description = p.Description,
            FullDescription = p.FullDescription,
            Price = p.Price,
            Quantity = p.Quantity,
            Category = p.SubCategory?.Category != null ? new CategoryDto()
            {
                Id = p.SubCategory.Category.Id,
                Name = p.SubCategory.Category.Name,
            } : null,
            SubCategory = p.SubCategory != null ? SubCategoryMapper.ToDto(p.SubCategory) : null,
            ProductImages = ToImageDtos(p),
            // Le review pubbliche non espongono l'email dell'autore.
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
            Manufacturer = p.Manufacturer != null ? ManufacturerMapper.ToDto(p.Manufacturer) : null,
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
            SubCategory = p.SubCategory != null ? SubCategoryMapper.ToDto(p.SubCategory) : null,
            ProductImages = ToImageDtos(p)
        };

        private static List<ProductImageDto> ToImageDtos(Product p) =>
            p.ProductImages?.Select(img => new ProductImageDto { ImageUrl = img.ImageUrl }).ToList()
            ?? new List<ProductImageDto>();
    }
}
