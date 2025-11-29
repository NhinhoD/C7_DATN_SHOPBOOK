using ShopThueBanSach.Server.Entities.Relationships;

namespace ShopThueBanSach.Server.Entities
{
    public class Promotion
    {
        public string PromotionId { get; set; } = Guid.NewGuid().ToString();
        public string PromotionName { get; set; }
        public double DiscountPercentage { get; set; } //10%
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Many-to-many with SaleBook
        public ICollection<PromotionSaleBook> PromotionSaleBooks { get; set; }
    }
}
