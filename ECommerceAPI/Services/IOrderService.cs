using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDto?> CreateOrderAsync(CreateOrderDto dto, int userId);
        Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(int userId);
    }
}