using VTInformatica.DTOs.Manufacturer;

namespace VTInformatica.Interfaces
{
    public interface IManufacturerService
    {
        Task<GetManufacturerResponseDto> GetAllAsync();
        Task<ManufacturerDto?> GetByIdAsync(int id);
        Task<ManufacturerDto> CreateAsync(CreateManufacturerDto dto);
        Task<bool> UpdateAsync(int id, ManufacturerDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
