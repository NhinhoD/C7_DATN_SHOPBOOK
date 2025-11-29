using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Entities.ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.BooksModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly AppDBContext _context;

        public AuthorService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<AuthorDto>> GetAllAsync()
        {
            return await _context.Authors
                .Select(a => new AuthorDto
                {
                    AuthorId = a.AuthorId,
                    Name = a.Name,
                    Description = a.Description
                })
                .ToListAsync();
        }

        public async Task<AuthorDto?> GetByIdAsync(string id)
        {
            var author = await _context.Authors.FindAsync(id);
            return author == null ? null : new AuthorDto
            {
                AuthorId = author.AuthorId,
                Name = author.Name,
                Description = author.Description
            };
        }

        public async Task<AuthorDto?> CreateAsync(CreateAuthorDto dto)
        {
            var entity = new Author
            {
                AuthorId = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Authors.Add(entity);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.AuthorId);
        }

        public async Task<AuthorDto?> UpdateAsync(string id, AuthorUpdateDto dto)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null) return null;

            author.Name = dto.Name;
            author.Description = dto.Description;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null) return false;

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckNameExistsAsync(string name, string? excludeId = null)
        {
            var query = _context.Authors.Where(a => a.Name.ToLower() == name.Trim().ToLower());
            if (!string.IsNullOrEmpty(excludeId))
                query = query.Where(a => a.AuthorId != excludeId);
            return await query.AnyAsync();
        }
    }
}
