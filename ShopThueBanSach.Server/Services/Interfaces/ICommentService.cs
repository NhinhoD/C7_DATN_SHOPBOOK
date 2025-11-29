using ShopThueBanSach.Server.Models.CommentModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetAllAsync(string bookId);
        Task<CommentDto?> GetByIdAsync(string id);

        // ✅ Đã bỏ userId vì service tự lấy từ token
        Task<CommentDto> CreateAsync(CommentDto dto);

        Task<bool> DeleteAsync(string id);
		// ❌ bỏ userId khỏi tham số
		Task<List<CommentDto>> GetAllCommentsAsync();
	}

}
