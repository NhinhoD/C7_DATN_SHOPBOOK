namespace ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Daily
{
    public class RentOrderInDayDto
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public DateTime ActualReturnDate { get; set; }
        public decimal TotalValue { get; set; }
    }
}
