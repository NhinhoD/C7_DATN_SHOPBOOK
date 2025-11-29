using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Daily
{
    public class DailySaleBookStatisticsDto
    {
        public DateTime CreatedDate { get; set; }
        public int OrdersToday { get; set; }
        public decimal TotalValueToday { get; set; }
        public List<OrderStatus> Statuses { get; set; } = new();
        public List<SaleOrderInDayDto> Orders { get; set; } = new(); // ✅ Thêm dòng này
    }
}
