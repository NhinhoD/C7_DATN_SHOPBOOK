namespace ShopThueBanSach.Server.Models.BooksModel
{
    public class CategoryDto
    {
        public string CategoryId { get; set; } = Guid.NewGuid().ToString(); // Khởi tạo ID nếu cần
        public string CategoryName { get; set; }
        public string? Description { get; set; }
    }
}
