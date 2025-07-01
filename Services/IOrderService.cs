using dot_net_web_api.DTOs;

namespace dot_net_web_api.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<bool> UpdateOrderStatusAsync(int id, UpdateOrderDto updateOrderDto);
        Task<bool> DeleteOrderAsync(int id);
        Task<bool> CancelOrderAsync(int id);
    }
}