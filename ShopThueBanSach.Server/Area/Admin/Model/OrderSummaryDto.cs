namespace ShopThueBanSach.Server.Area.Admin.Model
{
    public class OrderSummaryDto
    {
        // Sale‑Order
        public int TotalSaleOrders { get; set; }
        public decimal TotalSaleAmount { get; set; }

        // Rent‑Order
        public int TotalRentOrders { get; set; }
        public decimal TotalRentAmount { get; set; }

        // Gộp chung
        public int TotalOrders => TotalSaleOrders + TotalRentOrders;
        public decimal GrandTotalAmount => TotalSaleAmount + TotalRentAmount;
    }
}
