using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.SlideModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
	public class SlideService : ISlideService
	{
		private readonly AppDBContext _context;
		private readonly IPhotoService _photoService;

		public SlideService(AppDBContext context, IPhotoService photoService)
		{
			_context = context;
			_photoService = photoService;
		}


		public async Task<List<Slide>> GetAllAsync()
		{
			return await _context.Slides.ToListAsync();
		}

		public async Task<Slide?> GetByIdAsync(string id)
		{
			return await _context.Slides.FindAsync(id);
		}

		public async Task<Slide> CreateAsync(SlideDto dto)
		{
			var (imageUrl, publicId) = await _photoService.UploadImageAsync(dto.ImageFile, "Slides");

			var slide = new Slide
			{
				ImageUrl = imageUrl,
				LinkUrl = dto.LinkUrl
			};

			_context.Slides.Add(slide);
			await _context.SaveChangesAsync();
			return slide;
		}


		public async Task<bool> DeleteAsync(string id)
		{
			var slide = await _context.Slides.FindAsync(id);
			if (slide == null) return false;

			if (!string.IsNullOrEmpty(slide.ImageUrl))
			{
				var publicId = Path.GetFileNameWithoutExtension(new Uri(slide.ImageUrl).AbsolutePath);
				await _photoService.DeleteImageAsync("Slides/" + publicId);
			}

			_context.Slides.Remove(slide);
			await _context.SaveChangesAsync();
			return true;
		}


	}
}
