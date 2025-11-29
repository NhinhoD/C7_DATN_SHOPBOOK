namespace ShopThueBanSach.Server.Models.BooksModel
{
    public class FavoriteRentBookDto
    {
        public string RentBookId { get; set; }
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string UserName { get; set; }
    }

}


