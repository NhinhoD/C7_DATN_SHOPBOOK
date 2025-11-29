namespace ShopThueBanSach.Server.Models.BooksModel.DiscountCode
{
    // Dùng cho UPDATE
    public class UpdateDiscountCodeDto
    {
        public string? DiscountCodeName { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? AvailableQuantity { get; set; }
        public int? RequiredPoints { get; set; }
        public double? DiscountValue { get; set; }
    }
}
