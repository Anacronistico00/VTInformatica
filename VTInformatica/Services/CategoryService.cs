using Microsoft.EntityFrameworkCore;
using VTInformatica.Data;
using VTInformatica.DTOs.Category;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Interfaces;
using VTInformatica.Models;

namespace VTInformatica.Services
{
    public class CategoryService :ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            return await _context.Categories.Include(c => c.SubCategories).Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                SubCategories = c.SubCategories != null ? c.SubCategories.Select(sc => new GetSubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                }).ToList() : null
            }).ToListAsync();
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var c = await _context.Categories.Include(c => c.SubCategories).FirstOrDefaultAsync(c => c.Id == id);
            return c == null ? null : new CategoryDto { 
                Id = c.Id, 
                Name = c.Name,
                SubCategories = c.SubCategories != null ? c.SubCategories.Select(sc => new GetSubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                }).ToList() : null
            };
        }

        public async Task<CategoryDto> CreateAsync(CategoryDto dto)
        {
            var category = new Category { Name = dto.Name };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            dto.Id = category.Id;
            return dto;
        }

        public async Task<bool> UpdateAsync(int id, CategoryDto dto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;
            category.Name = dto.Name;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
