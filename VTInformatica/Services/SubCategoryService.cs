using Microsoft.EntityFrameworkCore;
using Serilog;
using VTInformatica.Data;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Interfaces;
using VTInformatica.Models;

namespace VTInformatica.Services
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly ApplicationDbContext _context;

        public SubCategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubCategoryDto>> GetAllAsync()
        {
            try
            {
                var subCategories = await _context.SubCategories
                    .Include(sc => sc.Category)
                    .Select(sc => new SubCategoryDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        CategoryId = sc.CategoryId
                     })
                     .ToListAsync();
                return subCategories;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during getting all SubCategories");
                throw;
            }
        }

        public async Task<SubCategoryDto> GetByIdAsync(int id)
        {
            try
            {
                var subCategory = await _context.SubCategories
                    .Include(sc => sc.Category)
                    .Where(sc => sc.Id == id)
                    .Select(sc => new SubCategoryDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        CategoryId = sc.CategoryId
                    })
                    .FirstOrDefaultAsync();

                return subCategory;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error during getting Product");
                throw;
            }
        }

        public async Task<SubCategoryDto> CreateAsync(SubCategoryDto dto)
        {
            try
            {
                var subCategory = new SubCategory
                {
                    Name = dto.Name,
                    CategoryId = dto.CategoryId
                };

                _context.SubCategories.Add(subCategory);
                await _context.SaveChangesAsync();

                dto.Id = subCategory.Id;
                return dto;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during CreateAsync for SubCategory");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(int id, SubCategoryDto dto)
        {
            try
            {
                var subCategory = await _context.SubCategories.FindAsync(id);
                if (subCategory == null) return false;

                subCategory.Name = dto.Name;
                subCategory.CategoryId = dto.CategoryId;

                _context.SubCategories.Update(subCategory);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error during UpdateAsync for SubCategory with id {id}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var subCategory = await _context.SubCategories.FindAsync(id);
                if (subCategory == null) return false;

                _context.SubCategories.Remove(subCategory);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error during DeleteAsync for SubCategory with id {id}");
                throw;
            }
        }
    }
}
