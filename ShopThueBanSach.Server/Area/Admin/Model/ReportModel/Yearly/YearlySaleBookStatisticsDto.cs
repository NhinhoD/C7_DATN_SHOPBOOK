using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Yearly
{
    public class YearlySaleBookStatisticsDto
    {
        public int Year { get; set; }
        public List<YearlySaleMonthDataDto> MonthlyData { get; set; } = new();
    }

    public class YearlySaleMonthDataDto
    {
        public int Month { get; set; }
        public int Orders { get; set; }
        public decimal TotalValue { get; set; }
        public List<DateTime> CreatedDates { get; set; } = new();
    }

}
