namespace ShopThueBanSach.Server.Models.BooksModel.RentBooks
{
    public class RentBookDto
    {
        public string RentBookId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Publisher { get; set; }
        public int PageCount { get; set; }
        public string? Translator { get; set; }
        public string? PackagingSize { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
    
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsHidden { get; set; }  // <-- thêm property này
        public List<string> AuthorIds { get; set; }
        public List<string> CategoryIds { get; set; }
    }

}
