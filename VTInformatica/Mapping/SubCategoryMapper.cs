using System.Linq.Expressions;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Models;

namespace VTInformatica.Mapping
{
    public static class SubCategoryMapper
    {
        public static readonly Expression<Func<SubCategory, SubCategoryDto>> Projection = sc => new SubCategoryDto
        {
            Id = sc.Id,
            Name = sc.Name,
            CategoryId = sc.CategoryId
        };

        private static readonly Func<SubCategory, SubCategoryDto> Compiled = Projection.Compile();

        public static SubCategoryDto ToDto(SubCategory subCategory) => Compiled(subCategory);
    }
}
