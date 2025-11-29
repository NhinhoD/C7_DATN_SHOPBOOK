using System.Text.Json.Serialization;

namespace ShopThueBanSach.Server.Models.BooksModel.SaleBooks
{
    public class SaleBookDto
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] // (System.Text.Json)
        public string SaleBookId { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }
        public string Publisher { get; set; }
        public int PageCount { get; set; }
        public string? Translator { get; set; }
        public string? PackagingSize { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; } // Thêm giá sau khuyến mãi
        public string? PromotionName { get; set; } // Tên khuyến mãi nếu có
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        //public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsHidden { get; set; }
        public List<string> AuthorIds { get; set; }
        public List<string> CategoryIds { get; set; }
        public List<string>? PromotionIds { get; set; } // Nhiều promotion có thể null
    }
}
