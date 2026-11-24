using System.Linq.Expressions;
using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Models;

namespace VTInformatica.Mapping
{
    public static class CategoryMapper
    {
        public static readonly Expression<Func<Category, CategoryDto>> Projection = c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            SubCategories = c.SubCategories!.Select(sc => new GetSubCategoryDto { Id = sc.Id, Name = sc.Name }).ToList()
        };
    }
}
