using System.ComponentModel.DataAnnotations;

namespace ShopThueBanSach.Server.Models.BooksModel.SaleBooks
{
    public class UpdateSaleBookDto
    {
      


        public string Title { get; set; }

        public string? Description { get; set; }


        public string Publisher { get; set; }

        public string? Translator { get; set; }
        public string? Size { get; set; }

        public int Pages { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public bool IsHidden { get; set; }

        public List<string> AuthorIds { get; set; }
        public List<string> CategoryIds { get; set; }

        public List<string>? PromotionIds { get; set; } = new();

        public IFormFile? ImageFile { get; set; } // ✅ dùng để upload ảnh
    }
}
