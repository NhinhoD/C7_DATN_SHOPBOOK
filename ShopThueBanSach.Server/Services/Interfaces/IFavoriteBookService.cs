using ShopThueBanSach.Server.Models.BooksModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IFavoriteBookService
    {
        Task<List<FavoriteBookDto>> GetFavoriteBooksAsync(string userId);
        Task<bool> AddFavoriteBookAsync(string userId, string saleBookId);
        Task<bool> RemoveFavoriteBookAsync(string userId, string saleBookId);
    }
}
