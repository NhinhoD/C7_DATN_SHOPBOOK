using ShopThueBanSach.Server.Entities.ShopThueBanSach.Server.Entities;

namespace ShopThueBanSach.Server.Entities.Relationships
{
    public class AuthorRentBook
    {
       
            public string AuthorId { get; set; }
            public Author Author { get; set; }

            public string RentBookId { get; set; }
            public RentBook RentBook { get; set; }
        
    }

}
