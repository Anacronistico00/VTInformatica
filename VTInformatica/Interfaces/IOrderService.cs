using VTInformatica.DTOs.Order;

namespace VTInformatica.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllAsync();
        Task<OrderDto?> GetByIdAsync(int id);
        Task<List<GetOrderDto>> GetOrdersByUserIdAsync(string userId);
        Task<OrderDto> CreateOrderFromCartAsync(string userId);
        Task<bool> SoftDeleteAsync(int id);
    }
}
