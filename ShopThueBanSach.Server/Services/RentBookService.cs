using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities.Relationships;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using ShopThueBanSach.Server.Models.BooksModel.RentBooks;

namespace ShopThueBanSach.Server.Services
{
    public class RentBookService : IRentBookService
    {
		private readonly AppDBContext _context;
		private readonly IWebHostEnvironment _env;
		private readonly IPhotoService _photoService;

		public RentBookService(AppDBContext context, IWebHostEnvironment env, IPhotoService photoService)
		{
			_context = context;
			_env = env;
			_photoService = photoService;
		}

		public async Task<List<RentBookDto>> GetAllAsync()
        {
            return await _context.RentBooks
                .Include(r => r.AuthorRentBooks)
                .Include(r => r.CategoryRentBooks)
                .Select(r => new RentBookDto
                {
                    RentBookId = r.RentBookId,
                    Title = r.Title,
                    Description = r.Description,
                    Publisher = r.Publisher,
                    PageCount = r.Pages,
                    Translator = r.Translator,
                    PackagingSize = r.Size,
                    Price = r.Price,
                    ImageUrl = r.ImageUrl,
                    Quantity = r.Quantity,
                    
                    IsHidden = r.IsHidden,
                    AuthorIds = r.AuthorRentBooks.Select(ar => ar.AuthorId).ToList(),
                    CategoryIds = r.CategoryRentBooks.Select(cr => cr.CategoryId).ToList()
                }).ToListAsync();
        }

        public async Task<RentBookDto?> GetByIdAsync(string id)
        {
            var r = await _context.RentBooks
                .Include(x => x.AuthorRentBooks)
                .Include(x => x.CategoryRentBooks)
                .FirstOrDefaultAsync(x => x.RentBookId == id);

            if (r == null) return null;

            return new RentBookDto
            {
                RentBookId = r.RentBookId,
                Title = r.Title,
                Description = r.Description,
                Publisher = r.Publisher,
                PageCount = r.Pages,
                Translator = r.Translator,
                PackagingSize = r.Size,
                Price = r.Price,
                ImageUrl = r.ImageUrl,
                Quantity = r.Quantity,
              
                IsHidden = r.IsHidden,
                AuthorIds = r.AuthorRentBooks.Select(ar => ar.AuthorId).ToList(),
                CategoryIds = r.CategoryRentBooks.Select(cr => cr.CategoryId).ToList()
            };
        }

		public async Task<string> CreateAsync(CreateRentBookDto dto, IFormFile? imageFile)
		{
			//string? imageUrl = null;



			var rentBook = new RentBook
			{
				RentBookId = Guid.NewGuid().ToString(),
				Title = dto.Title,
				Description = dto.Description,
				Publisher = dto.Publisher,
				Translator = dto.Translator,
				Size = dto.Size,
				Pages = dto.Pages,
				Price = dto.Price,
				//Quantity = dto.Quantity,
				IsHidden = dto.IsHidden,
				AuthorRentBooks = dto.AuthorIds.Select(id => new AuthorRentBook { AuthorId = id }).ToList(),
				CategoryRentBooks = dto.CategoryIds.Select(id => new CategoryRentBook { CategoryId = id }).ToList()
			};
			// ✅ Upload ảnh bằng Cloudinary
			if (dto.ImageFile != null && dto.ImageFile.Length > 0)
			{
				var (imageUrl, publicId) = await _photoService.UploadImageAsync(dto.ImageFile, "rentbook");
				rentBook.ImageUrl = imageUrl;
			}
			_context.RentBooks.Add(rentBook);
			await _context.SaveChangesAsync();
			return rentBook.RentBookId;
		}


		public async Task<bool> UpdateAsync(string id, UpdateRentBookDto dto)
		{
			var rentBook = await _context.RentBooks
				.Include(r => r.AuthorRentBooks)
				.Include(r => r.CategoryRentBooks)
				.FirstOrDefaultAsync(r => r.RentBookId == id);

			if (rentBook == null) return false;

			rentBook.Title = dto.Title;
			rentBook.Description = dto.Description;
			rentBook.Publisher = dto.Publisher;
			rentBook.Translator = dto.Translator;
			rentBook.Size = dto.Size;
			rentBook.Pages = dto.Pages;
			rentBook.Price = dto.Price;
			//rentBook.Quantity = dto.Quantity;
			rentBook.IsHidden = dto.IsHidden;

			// ✅ Xử lý ảnh Cloudinary
			if (dto.ImageFile != null && dto.ImageFile.Length > 0)
			{
				// Xóa ảnh cũ
				if (!string.IsNullOrEmpty(rentBook.ImageUrl))
				{
					var publicId = System.IO.Path.GetFileNameWithoutExtension(new Uri(rentBook.ImageUrl).AbsolutePath);
					await _photoService.DeleteImageAsync("rentbook/" + publicId);
				}

				// Upload ảnh mới
				var (imageUrl, publicIdNew) = await _photoService.UploadImageAsync(dto.ImageFile, "rentbook");
				if (!string.IsNullOrEmpty(imageUrl))
					rentBook.ImageUrl = imageUrl;
			}

			// ✅ Cập nhật Author & Category
			rentBook.AuthorRentBooks.Clear();
			rentBook.AuthorRentBooks = dto.AuthorIds.Select(id => new AuthorRentBook { AuthorId = id }).ToList();

			rentBook.CategoryRentBooks.Clear();
			rentBook.CategoryRentBooks = dto.CategoryIds.Select(id => new CategoryRentBook { CategoryId = id }).ToList();

			await _context.SaveChangesAsync();
			return true;
		}


		public async Task<bool> DeleteAsync(string id)
		{
			var rentBook = await _context.RentBooks.FindAsync(id);
			if (rentBook == null) return false;

			// ✅ Xoá ảnh khỏi Cloudinary nếu có
			if (!string.IsNullOrEmpty(rentBook.ImageUrl))
			{
				var publicId = System.IO.Path.GetFileNameWithoutExtension(new Uri(rentBook.ImageUrl).AbsolutePath);
				await _photoService.DeleteImageAsync("rentbook/" + publicId);
			}

			_context.RentBooks.Remove(rentBook);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> SetVisibilityAsync(string id, bool isHidden)
        {
            var rentBook = await _context.RentBooks.FindAsync(id);
            if (rentBook == null) return false;
            rentBook.IsHidden = isHidden;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckTitleExistsAsync(string title, string? excludeId = null)
        {
            var query = _context.RentBooks.Where(rb => rb.Title.ToLower() == title.Trim().ToLower());
            if (!string.IsNullOrEmpty(excludeId))
                query = query.Where(rb => rb.RentBookId != excludeId);
            return await query.AnyAsync();
        }
    }
}
