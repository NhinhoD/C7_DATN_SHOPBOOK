using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Monthly
{
    public class MonthlySaleBookStatisticsDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public List<SaleDayDataDto> DailyData { get; set; } = new();
    }
    public class SaleDayDataDto
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalValue { get; set; }
    }


}
