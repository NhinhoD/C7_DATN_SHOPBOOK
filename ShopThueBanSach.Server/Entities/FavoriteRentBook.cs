namespace ShopThueBanSach.Server.Entities
{
    public class FavoriteRentBook
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public string RentBookId { get; set; }
        public RentBook RentBook { get; set; }

        public DateTime FavoritedAt { get; set; } = DateTime.UtcNow;
    }
}
