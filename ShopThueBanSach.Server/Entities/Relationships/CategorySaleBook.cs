namespace ShopThueBanSach.Server.Entities.Relationships
{
    public class CategorySaleBook
    {
        public string CategoryId { get; set; }
        public Category Category { get; set; }

        public string SaleBookId { get; set; }
        public SaleBook SaleBook { get; set; }
    }


}
