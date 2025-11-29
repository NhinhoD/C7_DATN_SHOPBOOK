using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Models.PaymentMethod;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
    public class MoMoCallbackService : IMoMoCallbackService
    {
        private readonly AppDBContext _context;

        public MoMoCallbackService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> HandleCallbackAsync(MoMoResult result)
        {
            if (string.IsNullOrEmpty(result.OrderId))
                return new BadRequestObjectResult("Thiếu orderId");

            var order = await _context.RentOrders.FirstOrDefaultAsync(o => o.OrderId == result.OrderId);
            if (order == null)
                return new NotFoundObjectResult("Không tìm thấy đơn hàng");

            if (result.ResultCode == 0)
            {
                order.Status = Models.OrderStatus.Confirmed;
                order.OrderDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return new OkObjectResult("Thanh toán thành công");
            }

            order.Status = Models.OrderStatus.Canceled;
            await _context.SaveChangesAsync();
            return new OkObjectResult("Thanh toán thất bại");
        }
    }
}
