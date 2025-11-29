using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.BooksModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
	public class RentBookItemService : IRentBookItemService
	{
		private readonly AppDBContext _context;

		public RentBookItemService(AppDBContext context)
		{
			_context = context;
		}

		public async Task<List<RentBookItemDto>> GetAllAsync()
		{
			return await _context.RentBookItems
				.Include(x => x.RentBook)
				.Select(x => new RentBookItemDto
				{
					RentBookItemId = x.RentBookItemId,
					RentBookId = x.RentBookId,
					RentBookTitle = x.RentBook != null ? x.RentBook.Title : null,
					Status = x.Status,
					StatusDescription = x.StatusDescription,
					Condition = x.Condition,
					IsHidden = x.IsHidden
				})
				.ToListAsync();
		}

		public async Task<RentBookItemDto?> GetByIdAsync(string id)
		{
			var item = await _context.RentBookItems
				.Include(x => x.RentBook)
				.FirstOrDefaultAsync(x => x.RentBookItemId == id);

			if (item == null) return null;

			return new RentBookItemDto
			{
				RentBookItemId = item.RentBookItemId,
				RentBookId = item.RentBookId,
				RentBookTitle = item.RentBook?.Title,
				Status = item.Status,
				StatusDescription = item.StatusDescription,
				Condition = item.Condition,
				IsHidden = item.IsHidden
			};
		}

		public async Task<RentBookItemDto?> CreateAsync(RentBookItemDto dto)
		{
			var rentBook = await _context.RentBooks
				.FirstOrDefaultAsync(r => r.RentBookId == dto.RentBookId);

			if (rentBook == null)
				throw new InvalidOperationException("RentBook không tồn tại.");

			var entity = new RentBookItem
			{
				RentBookId = dto.RentBookId,
				Condition = dto.Condition,
				Status = RentBookItemStatus.Available,
				StatusDescription = dto.StatusDescription,
				IsHidden = dto.IsHidden
			};

			_context.RentBookItems.Add(entity);

			// ✅ TĂNG RentBook.Quantity khi thêm mới RentBookItem
			rentBook.Quantity += 1;

			await _context.SaveChangesAsync();

			dto.RentBookItemId = entity.RentBookItemId;
			dto.RentBookTitle = rentBook.Title;
			return dto;
		}

		public async Task<RentBookItemDto?> UpdateAsync(string id, RentBookItemDto dto)
		{
			var entity = await _context.RentBookItems
.Include(rbi => rbi.RentBook)
				.FirstOrDefaultAsync(x => x.RentBookItemId == id);

			if (entity == null)
				return null;

			if (entity.RentBookId != dto.RentBookId)
			{
				// ✅ Nếu thay đổi RentBookId
				var oldRentBook = await _context.RentBooks.FirstOrDefaultAsync(r => r.RentBookId == entity.RentBookId);
				if (oldRentBook != null)
					oldRentBook.Quantity -= 1;

				var newRentBook = await _context.RentBooks.FirstOrDefaultAsync(r => r.RentBookId == dto.RentBookId);
				if (newRentBook == null)
					throw new InvalidOperationException("RentBook mới không tồn tại.");

				newRentBook.Quantity += 1;
				entity.RentBookId = dto.RentBookId;
			}

			entity.Status = dto.Status;
			entity.StatusDescription = dto.StatusDescription;
			entity.Condition = dto.Condition;
			entity.IsHidden = dto.Condition >= 70;

			await _context.SaveChangesAsync();

			return await GetByIdAsync(id);
		}

		public async Task<bool> DeleteAsync(string id)
		{
			var entity = await _context.RentBookItems
				.Include(rbi => rbi.RentBook)
				.FirstOrDefaultAsync(x => x.RentBookItemId == id);

			if (entity == null)
				return false;

			// ✅ GIẢM RentBook.Quantity khi xóa RentBookItem
			if (entity.RentBook != null)
			{
				entity.RentBook.Quantity -= 1;
			}

			_context.RentBookItems.Remove(entity);
			await _context.SaveChangesAsync();
			return true;
		}
	}

}
