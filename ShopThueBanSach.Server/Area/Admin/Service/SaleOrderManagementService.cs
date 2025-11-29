using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Area.Admin.Service
{
    public class SaleOrderManagementService : ISaleOrderManagementService
    {
        private readonly AppDBContext _context;

        public SaleOrderManagementService(AppDBContext context)
        {
            _context = context;
        }

        // Lấy tất cả đơn bán
        public async Task<List<SaleOrder>> GetAllSaleOrdersAsync()
        {
            return await _context.SaleOrders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // Lấy đơn bán theo trạng thái
        public async Task<List<SaleOrder>> GetSaleOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.SaleOrders
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // Lấy chi tiết đơn bán
        public async Task<SaleOrder?> GetSaleOrderByIdAsync(string orderId)
        {
            return await _context.SaleOrders
                .Include(o => o.Details)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<List<SaleOrderDetail>> GetSaleOrderDetailsAsync(string orderId)
        {
            return await _context.SaleOrderDetails
                .Where(d => d.OrderId == orderId)
                .ToListAsync();
        }

        // Cập nhật trạng thái đơn bán
        public async Task<bool> UpdateSaleOrderStatusAsync(string orderId, OrderStatus newStatus)
        {
            var order = await _context.SaleOrders.FindAsync(orderId);
            if (order == null) return false;

            if (!Enum.IsDefined(typeof(OrderStatus), newStatus)) return false;

            order.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        // Cập nhật tổng tiền nếu cần
        public async Task<bool> UpdateSaleOrderAmountAsync(string orderId, decimal newTotal)
        {
            var order = await _context.SaleOrders.FindAsync(orderId);
            if (order == null) return false;

            order.TotalAmount = newTotal;
            await _context.SaveChangesAsync();
            return true;
        }

        // Huỷ đơn hàng bán
        public async Task<bool> CancelSaleOrderAsync(string orderId)
        {
            var order = await _context.SaleOrders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = OrderStatus.Canceled;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
