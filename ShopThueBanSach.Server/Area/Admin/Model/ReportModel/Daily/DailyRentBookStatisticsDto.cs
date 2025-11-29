using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Daily
{
    public class DailyRentBookStatisticsDto
    {
        public DateTime ActualReturnDate { get; set; }
        public int OrdersToday { get; set; }
        public decimal TotalValueToday { get; set; }
        public List<OrderStatus> Statuses { get; set; } = new(); // Giữ nguyên
        public List<RentOrderInDayDto> Orders { get; set; } = new(); // ✅ Thêm dòng này
    }
}
