using ShopThueBanSach.Server.Entities.Relationships;
using System.Text.Json.Serialization;

namespace ShopThueBanSach.Server.Entities
{
    public class RentBook
    {
        public string RentBookId { get; set; } = Guid.NewGuid().ToString();

        public string Title { get; set; }

        public string? Description { get; set; }

        public string? Publisher { get; set; }

        public string? Translator { get; set; }

        public string? Size { get; set; }

        public int Pages { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsHidden { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<AuthorRentBook> AuthorRentBooks { get; set; }

        public ICollection<CategoryRentBook> CategoryRentBooks { get; set; }
        [JsonIgnore]
        public ICollection<RentBookItem> RentBookItems { get; set; }
        public ICollection<FavoriteRentBook> FavoriteRentBooks { get; set; }

    }
}
