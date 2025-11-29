using ShopThueBanSach.Server.Entities.Relationships;

namespace ShopThueBanSach.Server.Entities
{
    public class Category
    {
        public string CategoryId { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; }

        public string? Description { get; set; }

        public ICollection<CategorySaleBook> CategorySaleBooks { get; set; }

        public ICollection<CategoryRentBook> CategoryRentBooks { get; set; }
    }
}
