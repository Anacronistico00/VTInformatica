using Microsoft.EntityFrameworkCore;
using VTInformatica.Data;
using VTInformatica.DTOs.SubCategory;
using VTInformatica.Exceptions;
using VTInformatica.Interfaces;
using VTInformatica.Models;

namespace VTInformatica.Services
{
    // Nessun try/catch con log + rethrow (AUDIT #11): le eccezioni le logga una sola volta GlobalExceptionHandler.
    public class SubCategoryService : ISubCategoryService
    {
        private readonly ApplicationDbContext _context;

        public SubCategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SubCategoryDto>> GetAllAsync()
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

        public async Task<SubCategoryDto> GetByIdAsync(int id)
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

        public async Task<SubCategoryDto> CreateAsync(SubCategoryDto dto)
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

        public async Task<bool> UpdateAsync(int id, SubCategoryDto dto)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null) return false;

            subCategory.Name = dto.Name;
            subCategory.CategoryId = dto.CategoryId;

            _context.SubCategories.Update(subCategory);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null) return false;

            // Verifica esplicita invece di affidarsi all'errore FK del DB (indipendente dal provider).
            // IgnoreQueryFilters: contano anche i prodotti eliminati, che mantengono lo storico ordini.
            if (await _context.Products.IgnoreQueryFilters().AnyAsync(p => p.SubCategoryId == id))
                throw new ResourceInUseException(
                    "La sottocategoria ha prodotti collegati (compresi quelli eliminati, che mantengono lo storico ordini): non può essere eliminata.");

            _context.SubCategories.Remove(subCategory);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
