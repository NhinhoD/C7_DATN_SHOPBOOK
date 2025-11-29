using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Monthly
{
    public class MonthlySaleBookStatisticsByMonthDto
    {
        public int Month { get; set; }
        public List<DateTime> CreatedDates { get; set; } = new();
        public int OrdersThisMonth { get; set; }
        public decimal TotalValueThisMonth { get; set; }
        public List<OrderStatus> Statuses { get; set; } = new();//
    }
}
