using Microsoft.EntityFrameworkCore;
using VTInformatica.Data;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.Exceptions;
using VTInformatica.Interfaces;
using VTInformatica.Mapping;
using VTInformatica.Models;

namespace VTInformatica.Services
{
    // Nessun try/catch con log + rethrow (AUDIT #11): le eccezioni le logga una sola volta GlobalExceptionHandler.
    public class ManufacturerService : IManufacturerService
    {
        private readonly ApplicationDbContext _context;
        public ManufacturerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GetManufacturerResponseDto> GetAllAsync()
        {
            var manufacturers = await _context.Manufacturers
                .Select(ManufacturerMapper.Projection).ToListAsync();

            return new GetManufacturerResponseDto
            {
                Message = "Manufacturers retrieved successfully",
                Manufacturers = manufacturers
            };
        }

        public async Task<ManufacturerDto?> GetByIdAsync(int id)
        {
            return await _context.Manufacturers
                .Where(m => m.ManufacturerId == id)
                .Select(ManufacturerMapper.Projection)
                .FirstOrDefaultAsync();
        }

        public async Task<ManufacturerDto> CreateAsync(CreateManufacturerDto dto)
        {
            var manufacturer = new Manufacturer
            {
                ManufacturerName = dto.ManufacturerName,
                ManufacturerLogo = dto.ManufacturerLogo,
                Products = new List<Product>()
            };

            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();

            return ManufacturerMapper.ToDto(manufacturer);
        }

        public async Task<bool> UpdateAsync(int id, ManufacturerDto dto)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(id);
            if (manufacturer == null) return false;

            manufacturer.ManufacturerName = dto.ManufacturerName;
            manufacturer.ManufacturerLogo = dto.ManufacturerLogo;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var manufacturer = await _context.Manufacturers.FindAsync(id);
            if (manufacturer == null) return false;

            // Verifica esplicita invece di affidarsi all'errore FK del DB (indipendente dal provider).
            // IgnoreQueryFilters: contano anche i prodotti eliminati, che mantengono lo storico ordini.
            if (await _context.Products.IgnoreQueryFilters().AnyAsync(p => p.ManufacturerId == id))
                throw new ResourceInUseException(
                    "Il produttore ha prodotti collegati (compresi quelli eliminati, che mantengono lo storico ordini): non può essere eliminato.");

            _context.Manufacturers.Remove(manufacturer);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
