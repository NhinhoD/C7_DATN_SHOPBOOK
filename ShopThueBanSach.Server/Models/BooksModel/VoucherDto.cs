namespace ShopThueBanSach.Server.Models.BooksModel
{
    public class VoucherDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string DiscountCodeName { get; set; }
        public int RequiredPoints { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
