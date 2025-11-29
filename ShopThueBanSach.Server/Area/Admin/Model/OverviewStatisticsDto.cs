namespace ShopThueBanSach.Server.Area.Admin.Model
{
    public class OverviewStatisticsDto
    {
        public int TotalRentBooks { get; set; }
        public int TotalSaleBooks { get; set; }
        public int TotalRentBookItems { get; set; }
        public int AvailableRentBookItems { get; set; }
        public decimal TotalRentBookValue { get; set; }
        public decimal TotalSaleBookValue { get; set; }
    }
}
