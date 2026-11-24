using VTInformatica.DTOs;
using VTInformatica.DTOs.Order;

namespace VTInformatica.Interfaces
{
    public interface IOrderService
    {
        Task<List<GetOrderDto>> GetAllAsync();
        Task<(List<GetOrderDto> Items, int Total)> GetPageAsync(PageQuery paging);
        Task<GetOrderDto?> GetByIdAsync(int id);
        Task<List<GetOrderDto>> GetOrdersByEmailAsync(string Email);
        Task<GetOrderDto> CreateOrderFromCartAsync(string userEmail, string? customerComment = null);
        Task<bool> SoftDeleteAsync(int id);
        Task<bool> RestoreAsync(int orderId);
    }
}
