using ShopThueBanSach.Server.Entities.ShopThueBanSach.Server.Entities;

namespace ShopThueBanSach.Server.Entities.Relationships
{
   
        public class AuthorSaleBook
        {
            public string AuthorId { get; set; }
            public Author Author { get; set; }

            public string SaleBookId { get; set; }
            public SaleBook SaleBook { get; set; }
        }
    }

