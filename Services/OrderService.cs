using dot_net_web_api.Data;
using dot_net_web_api.DTOs;
using dot_net_web_api.Models.Entities;
using dot_net_web_api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using dot_net_web_api.Enums;

namespace dot_net_web_api.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            // Get user information
            var user = await _context.Users.FindAsync(createOrderDto.UserId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            // Get products and calculate total amount
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var itemDto in createOrderDto.OrderItems)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null)
                {
                    throw new ArgumentException($"Product with ID {itemDto.ProductId} not found");
                }

                if (product.StockQuantity < itemDto.Quantity)
                {
                    throw new InvalidOperationException($"Not enough stock for product {product.Name}");
                }

                var orderItem = new OrderItem
                {
                    ProductName = product.Name ?? string.Empty,
                    Quantity = itemDto.Quantity,
                    Price = product.Price,
                    CreatedAt = DateTime.UtcNow
                };

                totalAmount += product.Price * itemDto.Quantity;
                orderItems.Add(orderItem);

                // Update product stock
                product.StockQuantity -= itemDto.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                _context.Products.Update(product);
            }

            // Create the order
            var order = new Order
            {
                UserId = createOrderDto.UserId,
                User = user,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending.ToString(),
                OrderItems = orderItems,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return MapToOrderDto(order);
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToOrderDto);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order != null ? MapToOrderDto(order) : null;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders.Select(MapToOrderDto);
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, UpdateOrderDto updateOrderDto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(updateOrderDto.Status))
            {
                order.Status = updateOrderDto.Status;
                order.UpdatedAt = DateTime.UtcNow;
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return false;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return false;
            }

            // Restore product quantities
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Name == item.ProductName);

                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    _context.Products.Update(product);
                }
            }

            order.Status = OrderStatus.Cancelled.ToString();
            order.UpdatedAt = DateTime.UtcNow;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return true;
        }

        private static OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                UserId = order.UserId,
                UserName = order.User?.Username ?? string.Empty,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductName = oi.ProductName,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    CreatedAt = oi.CreatedAt,
                    UpdatedAt = oi.UpdatedAt
                }).ToList(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
    }
}