using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.BooksModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly AppDBContext _context;

        public VoucherService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<string> ExchangePointsForVoucherAsync(string userId, string discountCodeId)
        {
            var user = await _context.Users.FindAsync(userId);
            var discountCode = await _context.DiscountCodes.FindAsync(discountCodeId);

            if (user == null || discountCode == null)
                return "Người dùng hoặc mã giảm giá không tồn tại";

            if (user.Points < discountCode.RequiredPoints)
                return "Không đủ điểm để đổi mã giảm giá này";

            if (discountCode.AvailableQuantity <= 0)
                return "Mã giảm giá đã hết lượt đổi";

            // Trừ điểm và giảm số lượng
            user.Points -= discountCode.RequiredPoints;
            discountCode.AvailableQuantity--;

            var voucher = new Voucher
            {
                Code = GenerateCode(),
                UserId = userId,
                DiscountCodeId = discountCodeId,
                IsUsed = false
            };

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            return "Đổi mã giảm giá thành công!";
        }

        public async Task<List<VoucherDto>> GetUserCurrentVouchers(string userId)
        {
            return await _context.Vouchers
                .Where(v => v.UserId == userId && !v.IsUsed)
                .Include(v => v.DiscountCode)
                .Select(v => new VoucherDto
                {
                    Id = v.Id,
                    Code = v.Code,
                    DiscountCodeName = v.DiscountCode.DiscountCodeName,
                    RequiredPoints = v.DiscountCode.RequiredPoints,
                    IsUsed = v.IsUsed,
                    CreatedAt = v.CreatedAt
                })
                .ToListAsync();
        }


        private string GenerateCode()
        {
            return Guid.NewGuid().ToString("N")[..8].ToUpper(); // Ví dụ: "ABC12345"
        }

        public async Task<List<VoucherHistoryDto>> GetUserVoucherHistory(string userId)
        {
            return await _context.Vouchers
                .Where(v => v.UserId == userId)
                .Include(v => v.DiscountCode)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new VoucherHistoryDto
                {
                    Code = v.Code,
                    DiscountCodeName = v.DiscountCode.DiscountCodeName,
                    DiscountValue = v.DiscountCode.DiscountValue,
                    RequiredPoints = v.DiscountCode.RequiredPoints,
                    IsUsed = v.IsUsed,
                    CreatedAt = v.CreatedAt
                })
                .ToListAsync();
        }
    }
}
