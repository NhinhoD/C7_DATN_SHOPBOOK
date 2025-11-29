using ShopThueBanSach.Server.Entities.Relationships;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ShopThueBanSach.Server.Entities
{
    namespace ShopThueBanSach.Server.Entities
    {
        public class Author
        {
            public string AuthorId { get; set; } = Guid.NewGuid().ToString();

            public string Name { get; set; }

            public string? Description { get; set; }

            public ICollection<AuthorSaleBook> AuthorSaleBooks { get; set; }

            public ICollection<AuthorRentBook> AuthorRentBooks { get; set; }
        }
    }


}
