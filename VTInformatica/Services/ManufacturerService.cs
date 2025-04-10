using Microsoft.EntityFrameworkCore;
using Serilog;
using VTInformatica.Data;
using VTInformatica.DTOs.Manufacturer;
using VTInformatica.Interfaces;
using VTInformatica.Models;

namespace VTInformatica.Services
{
    public class ManufacturerService : IManufacturerService
    {
        private readonly ApplicationDbContext _context;
        public ManufacturerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GetManufacturerResponseDto> GetAllAsync()
        {
            try
            {
                var manufacturers = await _context.Manufacturers
                    .Select(m => new ManufacturerDto
                    {
                        ManufacturerId = m.ManufacturerId,
                        ManufacturerName = m.ManufacturerName,
                        ManufacturerLogo = m.ManufacturerLogo
                    }).ToListAsync();

                return new GetManufacturerResponseDto
                {
                    Message = "Manufacturers retrieved successfully",
                    Manufacturers = manufacturers
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching all manufacturers");
                throw;
            }
        }

        public async Task<ManufacturerDto?> GetByIdAsync(int id)
        {
            try
            {
                var manufacturer = await _context.Manufacturers
                    .FirstOrDefaultAsync(m => m.ManufacturerId == id);
                if (manufacturer == null) return null;

                return new ManufacturerDto
                {
                    ManufacturerId = manufacturer.ManufacturerId,
                    ManufacturerName = manufacturer.ManufacturerName,
                    ManufacturerLogo = manufacturer.ManufacturerLogo
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error fetching manufacturer with id {id}");
                throw;
            }
        }

        public async Task<ManufacturerDto> CreateAsync(CreateManufacturerDto dto)
        {
            try
            {
                var manufacturer = new Manufacturer
                {
                    ManufacturerName = dto.ManufacturerName,
                    ManufacturerLogo = dto.ManufacturerLogo,
                    Products = new List<Product>()
                };

                _context.Manufacturers.Add(manufacturer);
                await _context.SaveChangesAsync();

                return new ManufacturerDto
                {
                    ManufacturerId = manufacturer.ManufacturerId,
                    ManufacturerName = manufacturer.ManufacturerName,
                    ManufacturerLogo = manufacturer.ManufacturerLogo
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating manufacturer");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(int id, ManufacturerDto dto)
        {
            try
            {
                var manufacturer = await _context.Manufacturers.FindAsync(id);
                if (manufacturer == null) return false;

                manufacturer.ManufacturerName = dto.ManufacturerName;
                manufacturer.ManufacturerLogo = dto.ManufacturerLogo;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error updating manufacturer with id {id}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var manufacturer = await _context.Manufacturers.FindAsync(id);
                if (manufacturer == null) return false;

                _context.Manufacturers.Remove(manufacturer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error deleting manufacturer with id {id}");
                throw;
            }
        }
    }
}
