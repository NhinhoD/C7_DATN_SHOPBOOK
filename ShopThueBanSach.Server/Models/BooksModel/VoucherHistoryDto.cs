namespace ShopThueBanSach.Server.Models.BooksModel
{
    public class VoucherHistoryDto
    {
        public string Code { get; set; }
        public string DiscountCodeName { get; set; }
        public double DiscountValue { get; set; }
        public int RequiredPoints { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
