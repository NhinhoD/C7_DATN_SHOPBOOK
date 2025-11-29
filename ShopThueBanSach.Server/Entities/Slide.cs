namespace ShopThueBanSach.Server.Entities
{
    public class Slide
    {
        public string SlideId { get; set; } = Guid.NewGuid().ToString();// Tương ứng MaSlide
        public string ImageUrl { get; set; } = string.Empty; // HinhAnh
        public string? LinkUrl { get; set; } // DuongDan
    }
}
