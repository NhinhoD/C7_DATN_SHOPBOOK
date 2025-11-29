namespace ShopThueBanSach.Server.Entities.Relationships
{
    //Đây là bảng trung gian (join table) giữa Promotion và SaleBook.

    //Dùng trong mối quan hệ nhiều-nhiều: 1 Promotion áp dụng cho nhiều SaleBook và ngược lại.


    public class PromotionSaleBook
    {
        public string PromotionId { get; set; }
        public Promotion Promotion { get; set; }

        public string SaleBookId { get; set; }
        public SaleBook SaleBook { get; set; }
    }
}
