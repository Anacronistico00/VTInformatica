using VTInformatica.DTOs.Order;

namespace VTInformatica.Interfaces
{
    public interface IOrderService
    {
        Task<List<GetOrderDto>> GetAllAsync();
        Task<GetOrderDto?> GetByIdAsync(int id);
        Task<List<GetOrderDto>> GetOrdersByEmailAsync(string Email);
        Task<GetOrderDto> CreateOrderFromCartAsync(string userEmail);
        Task<bool> SoftDeleteAsync(int id);
    }
}
