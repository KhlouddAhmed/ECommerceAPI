using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderResponseDto?> CreateOrderAsync(CreateOrderDto dto, int userId)
        {
            var order = new Order { UserId = userId };
            decimal total = 0;

            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null || product.Stock < item.Quantity) return null;

                product.Stock -= item.Quantity;
                total += product.Price * item.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = product.Price
                });
            }

            order.TotalPrice = total;
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return MapToDto(order);
        }

        public async Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return orders.Select(MapToDto);
        }

        private OrderResponseDto MapToDto(Order order) => new()
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            Items = order.OrderItems.Select(i => new OrderItemResponseDto
            {
                ProductName = i.Product?.Name ?? "",
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };
    }
}