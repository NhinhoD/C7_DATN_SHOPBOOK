namespace ShopThueBanSach.Server.Entities
{
    public class FavoriteBook
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public string SaleBookId { get; set; }
        public SaleBook SaleBook { get; set; }
    }
}
