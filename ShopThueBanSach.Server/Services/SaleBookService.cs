using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities.Relationships;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Models.BooksModel.SaleBooks;
using Microsoft.JSInterop.Infrastructure;

namespace ShopThueBanSach.Server.Services
{
    public class SaleBookService : ISaleBookService
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _env;
		private readonly IPhotoService _photoService;
		public SaleBookService(AppDBContext context, IWebHostEnvironment env, IPhotoService photoService)
        {
            _context = context;
            _env = env;
			_photoService = photoService;
		}

		public async Task<List<SaleBookDto>> GetAllAsync()
		{
			var saleBooks = await _context.SaleBooks
				.Include(sb => sb.AuthorSaleBooks)
				.Include(sb => sb.CategorySaleBooks)
				.Include(sb => sb.PromotionSaleBooks)
					.ThenInclude(psb => psb.Promotion)
				.ToListAsync();

			var result = saleBooks.Select(sb =>
			{
				// Lọc các promotion hợp lệ cho sách này
				var validPromotions = sb.PromotionSaleBooks
					.Select(psb => psb.Promotion)
					.Where(p => p != null && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
					.ToList();

				// Chọn promotion tốt nhất
				var bestPromotion = validPromotions
					.OrderByDescending(p => p.DiscountPercentage)
					.FirstOrDefault();

				return new SaleBookDto
				{
					SaleBookId = sb.SaleBookId,
					Title = sb.Title,
					Description = sb.Description,
					Publisher = sb.Publisher,
					Translator = sb.Translator,
					PackagingSize = sb.Size,
					PageCount = sb.Pages,
					Price = sb.Price,
					FinalPrice = bestPromotion != null
						? sb.Price * (1 - (decimal)(bestPromotion.DiscountPercentage / 100))
						: sb.Price,
					PromotionName = bestPromotion?.PromotionName,
					PromotionIds = validPromotions.Select(p => p.PromotionId).ToList(),
					Quantity = sb.Quantity,
					ImageUrl = sb.ImageUrl,
					IsHidden = sb.IsHidden,
					AuthorIds = sb.AuthorSaleBooks.Select(a => a.AuthorId).ToList(),
					CategoryIds = sb.CategorySaleBooks.Select(c => c.CategoryId).ToList()
				};
			}).ToList();

			return result;
		}


		public async Task<SaleBookDto?> GetByIdAsync(string id)
        {
            var sb = await _context.SaleBooks
                .Include(s => s.AuthorSaleBooks)
                .Include(s => s.CategorySaleBooks)
                .Include(s => s.PromotionSaleBooks).ThenInclude(psb => psb.Promotion)
                .FirstOrDefaultAsync(s => s.SaleBookId == id);

            if (sb == null) return null;

            var validPromotions = sb.PromotionSaleBooks
                .Select(psb => psb.Promotion)
                .Where(p => p != null && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
                .ToList();

            var bestPromotion = validPromotions
                .OrderByDescending(p => p.DiscountPercentage)
                .FirstOrDefault();

            return new SaleBookDto
            {
                SaleBookId = sb.SaleBookId,
                Title = sb.Title,
                Description = sb.Description,
                Publisher = sb.Publisher,
                Translator = sb.Translator,
                PackagingSize = sb.Size,
                PageCount = sb.Pages,
                Price = sb.Price,
                FinalPrice = bestPromotion != null
                    ? sb.Price * (1 - (decimal)(bestPromotion.DiscountPercentage / 100))
                    : sb.Price,
                PromotionName = bestPromotion?.PromotionName,
                PromotionIds = validPromotions.Select(p => p.PromotionId).ToList(),
                Quantity = sb.Quantity,
                ImageUrl = sb.ImageUrl,
                IsHidden = sb.IsHidden,
                AuthorIds = sb.AuthorSaleBooks.Select(a => a.AuthorId).ToList(),
                CategoryIds = sb.CategorySaleBooks.Select(c => c.CategoryId).ToList()
            };
        }


        public async Task<string> CreateAsync(CreateSaleBookDto dto)
        {
            var saleBook = new SaleBook
            {
                SaleBookId = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Description = dto.Description,
                Publisher = dto.Publisher,
                Translator = dto.Translator,
                Size = dto.Size,
                Pages = dto.Pages,
                Price = dto.Price,
                Quantity = dto.Quantity,
                IsHidden = dto.IsHidden,
                AuthorSaleBooks = dto.AuthorIds.Select(aid => new AuthorSaleBook { AuthorId = aid }).ToList(),
                CategorySaleBooks = dto.CategoryIds.Select(cid => new CategorySaleBook { CategoryId = cid }).ToList()
            };

			// ✅ Upload ảnh bằng Cloudinary
			if (dto.ImageUrl != null && dto.ImageUrl.Length > 0)
			{
				var (imageUrl, publicId) = await _photoService.UploadImageAsync(dto.ImageUrl, "SaleBooks");
				saleBook.ImageUrl = imageUrl;
			}

			_context.SaleBooks.Add(saleBook);


			// Xử lý promotions nếu có
			if (dto.PromotionIds != null && dto.PromotionIds.Any())
			{
				var validPromotions = await _context.Promotions
					.Where(p => dto.PromotionIds.Contains(p.PromotionId) &&
								p.StartDate <= DateTime.UtcNow &&
								p.EndDate >= DateTime.UtcNow)
					.ToListAsync();

				if (validPromotions.Any())
				{
					foreach (var promo in validPromotions)
					{
						_context.PromotionSaleBooks.Add(new PromotionSaleBook
						{
							PromotionId = promo.PromotionId,
							SaleBookId = saleBook.SaleBookId
						});
					}

					var maxDiscount = validPromotions.Max(p => p.DiscountPercentage);
					saleBook.FinalPrice = saleBook.Price * (1 - ((decimal)maxDiscount / 100));
				}
				else
				{
					// Không có promotion hợp lệ => giữ nguyên giá
					saleBook.FinalPrice = saleBook.Price;
				}
			}
			else
			{
				// Không nhập promotion => giữ nguyên giá
				saleBook.FinalPrice = saleBook.Price;
			}
			await _context.SaveChangesAsync();
            return saleBook.SaleBookId;
        }

        public async Task<bool> UpdateAsync(string id, UpdateSaleBookDto dto)
        {
            var saleBook = await _context.SaleBooks
                .Include(sb => sb.AuthorSaleBooks)
                .Include(sb => sb.CategorySaleBooks)
                .Include(sb => sb.PromotionSaleBooks)
                .FirstOrDefaultAsync(sb => sb.SaleBookId == id);

            if (saleBook == null) return false;

            saleBook.Title = dto.Title;
            saleBook.Description = dto.Description;
            saleBook.Publisher = dto.Publisher;
            saleBook.Translator = dto.Translator;
            saleBook.Size = dto.Size;
            saleBook.Pages = dto.Pages;
            saleBook.Price = dto.Price;
            saleBook.Quantity = dto.Quantity;
            saleBook.IsHidden = dto.IsHidden;


			if (dto.ImageFile != null && dto.ImageFile.Length > 0)
			{
				// Xóa ảnh cũ
				if (!string.IsNullOrEmpty(saleBook.ImageUrl))
				{
					var publicId = Path.GetFileNameWithoutExtension(new Uri(saleBook.ImageUrl).AbsolutePath);
					await _photoService.DeleteImageAsync("SaleBooks/" + publicId);
				}

				// Upload ảnh mới
				var (imageUrl, publicIdNew) = await _photoService.UploadImageAsync(dto.ImageFile, "SaleBooks");
				if (!string.IsNullOrEmpty(imageUrl))
					saleBook.ImageUrl = imageUrl;
			}

			// Cập nhật Promotions
			_context.PromotionSaleBooks.RemoveRange(saleBook.PromotionSaleBooks);

			// Lọc bỏ các PromotionId không hợp lệ như "string", null, rỗng
			var cleanPromotionIds = dto.PromotionIds?
				.Where(id => !string.IsNullOrWhiteSpace(id) && id != "string")
				.ToList();

			if (cleanPromotionIds != null && cleanPromotionIds.Any())
			{
				var validPromotions = await _context.Promotions
					.Where(p => cleanPromotionIds.Contains(p.PromotionId) &&
								p.StartDate <= DateTime.UtcNow &&
								p.EndDate >= DateTime.UtcNow)
					.ToListAsync();

				if (validPromotions.Any())
				{
					foreach (var promo in validPromotions)
					{
						_context.PromotionSaleBooks.Add(new PromotionSaleBook
						{
							PromotionId = promo.PromotionId,
							SaleBookId = saleBook.SaleBookId
						});
					}

					var maxDiscount = validPromotions.Max(p => p.DiscountPercentage);
					saleBook.FinalPrice = saleBook.Price * (1 - ((decimal)maxDiscount / 100));
				}
				else
				{
					saleBook.FinalPrice = saleBook.Price;
				}
			}
			else
			{
				// Nếu không có promotion hợp lệ => reset FinalPrice về gốc
				saleBook.FinalPrice = saleBook.Price;
			}
			// Cập nhật Author và Category
			saleBook.AuthorSaleBooks.Clear();
            saleBook.AuthorSaleBooks = dto.AuthorIds
                .Select(authorId => new AuthorSaleBook { AuthorId = authorId, SaleBookId = id })
                .ToList();

            saleBook.CategorySaleBooks.Clear();
            saleBook.CategorySaleBooks = dto.CategoryIds
                .Select(catId => new CategorySaleBook { CategoryId = catId, SaleBookId = id })
                .ToList();

            await _context.SaveChangesAsync();
            return true;
        }

		public async Task<bool> DeleteAsync(string id)
		{
			var saleBook = await _context.SaleBooks.FindAsync(id);
			if (saleBook == null) return false;

			// ✅ Xoá ảnh khỏi Cloudinary nếu có
			if (!string.IsNullOrEmpty(saleBook.ImageUrl))
			{
				var publicId = Path.GetFileNameWithoutExtension(new Uri(saleBook.ImageUrl).AbsolutePath);
				await _photoService.DeleteImageAsync("SaleBooks/" + publicId);
			}

			_context.SaleBooks.Remove(saleBook);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> SetVisibilityAsync(string id, bool isHidden)
        {
            var saleBook = await _context.SaleBooks.FindAsync(id);
            if (saleBook == null) return false;
            saleBook.IsHidden = isHidden;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckTitleExistsAsync(string title, string? excludeId = null)
        {
            var query = _context.SaleBooks.Where(b => b.Title.ToLower() == title.Trim().ToLower());
            if (!string.IsNullOrEmpty(excludeId))
                query = query.Where(b => b.SaleBookId != excludeId);
            return await query.AnyAsync();
        }
    }
}
