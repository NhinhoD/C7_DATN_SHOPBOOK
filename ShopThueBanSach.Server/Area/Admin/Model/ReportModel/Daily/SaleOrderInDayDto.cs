namespace ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Daily
{
    public class SaleOrderInDayDto
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public decimal TotalValue { get; set; }
    }
}
