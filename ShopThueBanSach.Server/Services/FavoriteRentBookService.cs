using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.BooksModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
    public class FavoriteRentBookService : IFavoriteRentBookService
    {
        private readonly AppDBContext _context;

        public FavoriteRentBookService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<FavoriteRentBookDto>> GetFavoritesByUserAsync(string userId)
        {
            return await _context.FavoriteRentBooks
                .Where(f => f.UserId == userId)
                .Include(f => f.RentBook)
                .Include(f => f.User)
                .Select(f => new FavoriteRentBookDto
                {
                    RentBookId = f.RentBookId,
                    Title = f.RentBook.Title,
                    ImageUrl = f.RentBook.ImageUrl,
                    Price = f.RentBook.Price,
                    UserName = f.User.UserName
                })
                .ToListAsync();
        }

        public async Task<bool> ToggleFavoriteAsync(string userId, string rentBookId)
        {
            var existing = await _context.FavoriteRentBooks
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RentBookId == rentBookId);

            if (existing != null)
            {
                _context.FavoriteRentBooks.Remove(existing);
            }
            else
            {
                var newFav = new FavoriteRentBook
                {
                    UserId = userId,
                    RentBookId = rentBookId
                };
                await _context.FavoriteRentBooks.AddAsync(newFav);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveFromFavoritesAsync(string userId, string rentBookId)
        {
            var fav = await _context.FavoriteRentBooks
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RentBookId == rentBookId);
            if (fav == null) return false;

            _context.FavoriteRentBooks.Remove(fav);
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
