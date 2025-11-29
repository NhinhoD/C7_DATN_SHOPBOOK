using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Entities.Relationships;
using ShopThueBanSach.Server.Models.BooksModel;
using ShopThueBanSach.Server.Models.BooksModel.Promotion;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly AppDBContext _context;

        public PromotionService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<bool> CreatePromotionAsync(PromotionDTO model)
        {
            var entity = new Promotion
            {
                PromotionName = model.PromotionName,
                DiscountPercentage = model.DiscountPercentage,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };
            _context.Promotions.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePromotionAsync(string id)
        {
            var promotion = await _context.Promotions
                .Include(p => p.PromotionSaleBooks)
                .ThenInclude(psb => psb.SaleBook)
                .FirstOrDefaultAsync(p => p.PromotionId == id);

            if (promotion == null) return false;

            // Reset FinalPrice của các sách
            foreach (var psb in promotion.PromotionSaleBooks)
            {
                psb.SaleBook.FinalPrice = psb.SaleBook.Price; // reset lại
            }

            // Xoá liên kết trung gian
            _context.PromotionSaleBooks.RemoveRange(promotion.PromotionSaleBooks);

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<List<PromotionDTO>> GetAllPromotionsAsync()
        {
            var promotions = await _context.Promotions.ToListAsync();
            return promotions.Select(p => new PromotionDTO
            {
                PromotionId = p.PromotionId,
                PromotionName = p.PromotionName,
                DiscountPercentage = p.DiscountPercentage,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            }).ToList();
        }

        public async Task<PromotionDTO?> GetPromotionByIdAsync(string id)
        {
            var p = await _context.Promotions.FindAsync(id);
            if (p == null) return null;
            return new PromotionDTO
            {
                PromotionId = p.PromotionId,
                PromotionName = p.PromotionName,
                DiscountPercentage = p.DiscountPercentage,
                StartDate = p.StartDate,
                EndDate = p.EndDate
            };
        }

        public async Task<bool> UpdatePromotionAsync(string id, PromotionDTO model)
        {
            var entity = await _context.Promotions.FindAsync(id);
            if (entity == null) return false;

            entity.PromotionName = model.PromotionName;
            entity.DiscountPercentage = model.DiscountPercentage;
            entity.StartDate = model.StartDate;
            entity.EndDate = model.EndDate;

            await _context.SaveChangesAsync();
            return true;
        }
        //public async Task<bool> ApplyPromotionToBooksAsync(ApplyPromotionDTO dto)
        //{
        //    var promotion = await _context.Promotions
        //        .Include(p => p.PromotionSaleBooks)
        //        .ThenInclude(psb => psb.SaleBook)
        //        .FirstOrDefaultAsync(p => p.PromotionId == dto.PromotionId);

        //    if (promotion == null) return false;

        //    // Xoá liên kết cũ
        //    var oldLinks = _context.PromotionSaleBooks
        //        .Where(psb => psb.PromotionId == dto.PromotionId);
        //    _context.PromotionSaleBooks.RemoveRange(oldLinks);

        //    var books = await _context.SaleBooks
        //        .Where(b => dto.SaleBookIds.Contains(b.SaleBookId))
        //        .ToListAsync();

        //    foreach (var book in books)
        //    {
        //        _context.PromotionSaleBooks.Add(new PromotionSaleBook
        //        {
        //            PromotionId = dto.PromotionId,
        //            SaleBookId = book.SaleBookId
        //        });

        //        // Cập nhật FinalPrice (ép kiểu rõ ràng giữa decimal - double)
        //        book.FinalPrice = book.Price * (decimal)((100 - promotion.DiscountPercentage) / 100.0);
        //    }

        //    await _context.SaveChangesAsync();
        //    return true;
        //}



        public async Task<bool> CheckNameExistsAsync(string promotionName, string? excludeId = null)
        {
            var query = _context.Promotions
                .Where(p => p.PromotionName.ToLower() == promotionName.Trim().ToLower());

            if (!string.IsNullOrEmpty(excludeId))
            {
                query = query.Where(p => p.PromotionId != excludeId);
            }

            return await query.AnyAsync();
        }

    }
}
//