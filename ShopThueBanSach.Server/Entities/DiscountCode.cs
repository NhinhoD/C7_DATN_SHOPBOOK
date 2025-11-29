namespace ShopThueBanSach.Server.Entities
{
    public class DiscountCode
    {
        public string DiscountCodeId { get; set; } = Guid.NewGuid().ToString();
        public string DiscountCodeName { get; set; }
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AvailableQuantity { get; set; } // Tổng số lượt có thể đổi
        public int RequiredPoints { get; set; }    // Số điểm cần để đổi
        public double DiscountValue { get; set; }  // Ví dụ: 0.1 = 10%

        public ICollection<Voucher> Vouchers { get; set; }
    }
}
