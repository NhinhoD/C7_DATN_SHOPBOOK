using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Models.CommentModel;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Services
{
	public class CommentService : ICommentService
	{
		private readonly AppDBContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public CommentService(AppDBContext context, IHttpContextAccessor httpContextAccessor)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
		}
		private string? GetCurrentUserId()
		{
			return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		}
		public async Task<List<CommentDto>> GetAllAsync(string bookId)
		{
			return await _context.Comments
				.Where(c => c.BookId == bookId)
				.OrderByDescending(c => c.CreatedDate)
				.Select(c => new CommentDto
				{
					CommentId = c.CommentId,
					Content = c.Content,
					CreatedDate = c.CreatedDate,
					BookId = c.BookId,
					ParentCommentId = c.ParentCommentId,
					CreatedById = c.CreatedById          // 🆕
				})
				.ToListAsync();
		}

		public async Task<CommentDto?> GetByIdAsync(string id)
		{
			var c = await _context.Comments.FindAsync(id);
			if (c == null) return null;

			return new CommentDto
			{
				CommentId = c.CommentId,
				Content = c.Content,
				CreatedDate = c.CreatedDate,
				BookId = c.BookId,
				ParentCommentId = c.ParentCommentId,
				CreatedById = c.CreatedById            // 🆕
			};
		}

		public async Task<CommentDto> CreateAsync(CommentDto dto)
		{
			var userId = GetCurrentUserId();
			if (string.IsNullOrEmpty(userId))
				throw new UnauthorizedAccessException("Không xác định được người dùng.");

			var comment = new Comment
			{
				CommentId = Guid.NewGuid().ToString(),
				Content = dto.Content,
				CreatedDate = DateTime.UtcNow,
				BookId = dto.BookId,
				ParentCommentId = dto.ParentCommentId,
				CreatedById = userId
			};

			_context.Comments.Add(comment);
			await _context.SaveChangesAsync();

			dto.CommentId = comment.CommentId;
			dto.CreatedDate = comment.CreatedDate;
			dto.CreatedById = userId;
			return dto;
		}

		public async Task<bool> DeleteAsync(string id)
		{
			var comment = await _context.Comments.FindAsync(id);
			if (comment == null) return false;
			_context.Comments.Remove(comment);
			await _context.SaveChangesAsync();
			return true;
		}
		public async Task<List<CommentDto>> GetAllCommentsAsync()
		{
			return await _context.Comments
				.OrderByDescending(c => c.CreatedDate)
				.Select(c => new CommentDto
				{
					CommentId = c.CommentId,
					Content = c.Content,
					CreatedDate = c.CreatedDate,
					BookId = c.BookId,
					ParentCommentId = c.ParentCommentId,
					CreatedById = c.CreatedById
				})
				.ToListAsync();
		}

	}
}