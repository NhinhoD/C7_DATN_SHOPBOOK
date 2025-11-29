namespace ShopThueBanSach.Server.Models.BooksModel.DiscountCode
{
    public class DiscountCodeDTO
    {
        public string? DiscountCodeId { get; set; }
        public string DiscountCodeName { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // ✅ Đổi "Quantity" thành đúng tên Entity: "AvailableQuantity"
        public int AvailableQuantity { get; set; }

        public int RequiredPoints { get; set; } // nếu cần cho chức năng đổi
        public double DiscountValue { get; set; }
        // ✅ Thêm danh sách mã Voucher
        public List<string> VoucherCodes { get; set; } = new();
    }
}
