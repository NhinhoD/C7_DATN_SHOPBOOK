using ShopThueBanSach.Server.Models.BooksModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IFavoriteRentBookService
    {
        Task<List<FavoriteRentBookDto>> GetFavoritesByUserAsync(string userId);
        Task<bool> ToggleFavoriteAsync(string userId, string rentBookId);
        Task<bool> RemoveFromFavoritesAsync(string userId, string rentBookId);
    }
}
