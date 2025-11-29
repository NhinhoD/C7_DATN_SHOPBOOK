using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Yearly
{
    public class YearlyRentBookStatisticsDto
    {
        public int Year { get; set; }
        public List<YearlyRentMonthDataDto> MonthlyData { get; set; } = new();
    }

    public class YearlyRentMonthDataDto
    {
        public int Month { get; set; }
        public int Orders { get; set; }
        public decimal TotalValue { get; set; }
        public List<DateTime> ReturnDates { get; set; } = new();
    }

}
