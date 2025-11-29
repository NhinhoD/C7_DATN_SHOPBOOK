namespace ShopThueBanSach.Server.Entities.Relationships
{
    public class CategoryRentBook
    {
        public string CategoryId { get; set; }
        public Category Category { get; set; }

        public string RentBookId { get; set; }
        public RentBook RentBook { get; set; }
    }
}
