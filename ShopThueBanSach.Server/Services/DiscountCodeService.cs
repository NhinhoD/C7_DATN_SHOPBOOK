using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Models.BooksModel.DiscountCode;

namespace ShopThueBanSach.Server.Services
{
    public class DiscountCodeService : IDiscountCodeService
    {
        private readonly AppDBContext _context;

        public DiscountCodeService(AppDBContext context)
        {
            _context = context;
        }
        public async Task<bool> CreateAsync(CreateDiscountCodeDto model)
        {
            var entity = new DiscountCode
            {
                DiscountCodeName = model.DiscountCodeName,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                AvailableQuantity = model.AvailableQuantity,
                RequiredPoints = model.RequiredPoints,
                DiscountValue = model.DiscountValue
            };

            _context.DiscountCodes.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var entity = await _context.DiscountCodes.FindAsync(id);
            if (entity == null) return false;

            _context.DiscountCodes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DiscountCodeDTO>> GetAllAsync()
        {
            return await _context.DiscountCodes
                .Include(dc => dc.Vouchers)
                .Select(d => new DiscountCodeDTO
                {
                    DiscountCodeId = d.DiscountCodeId,
                    DiscountCodeName = d.DiscountCodeName,
                    Description = d.Description,
                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    AvailableQuantity = d.AvailableQuantity,
                    RequiredPoints = d.RequiredPoints,
                    DiscountValue = d.DiscountValue,
                    VoucherCodes = d.Vouchers.Select(v => v.Code).ToList() // ✅ Thêm dòng này
                }).ToListAsync();
        }


        public async Task<DiscountCodeDTO?> GetByIdAsync(string id)
        {
            var d = await _context.DiscountCodes
                .Include(dc => dc.Vouchers)
                .FirstOrDefaultAsync(dc => dc.DiscountCodeId == id);

            if (d == null) return null;

            return new DiscountCodeDTO
            {
                DiscountCodeId = d.DiscountCodeId,
                DiscountCodeName = d.DiscountCodeName,
                Description = d.Description,
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                AvailableQuantity = d.AvailableQuantity,
                RequiredPoints = d.RequiredPoints,
                DiscountValue = d.DiscountValue,
                VoucherCodes = d.Vouchers.Select(v => v.Code).ToList() // ✅ Thêm dòng này
            };
        }


        public async Task<bool> UpdateAsync(string id, UpdateDiscountCodeDto model)
        {
            var entity = await _context.DiscountCodes.FindAsync(id);
            if (entity == null) return false;

            if (model.DiscountCodeName != null)
                entity.DiscountCodeName = model.DiscountCodeName;

            if (model.Description != null)
                entity.Description = model.Description;

            if (model.StartDate.HasValue)
                entity.StartDate = model.StartDate.Value;

            if (model.EndDate.HasValue)
                entity.EndDate = model.EndDate.Value;

            if (model.AvailableQuantity.HasValue)
                entity.AvailableQuantity = model.AvailableQuantity.Value;

            if (model.RequiredPoints.HasValue)
                entity.RequiredPoints = model.RequiredPoints.Value;

            if (model.DiscountValue.HasValue)
                entity.DiscountValue = model.DiscountValue.Value;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<DiscountCodeDTO>> GetAvailableForExchangeAsync()
        {
            var today = DateTime.UtcNow;
            return await _context.DiscountCodes
                .Include(dc => dc.Vouchers)
                .Where(dc => dc.AvailableQuantity > 0 && dc.StartDate <= today && dc.EndDate >= today)
                .Select(d => new DiscountCodeDTO
                {
                    DiscountCodeId = d.DiscountCodeId,
                    DiscountCodeName = d.DiscountCodeName,
                    Description = d.Description,
                    StartDate = d.StartDate,
                    EndDate = d.EndDate,
                    AvailableQuantity = d.AvailableQuantity,
                    RequiredPoints = d.RequiredPoints,
                    DiscountValue = d.DiscountValue,
                    VoucherCodes = d.Vouchers.Select(v => v.Code).ToList() // ✅ Tuỳ chọn
                }).ToListAsync();
        }


    }
}
