namespace ShopThueBanSach.Server.Models
{
    public class UpdateOrderStatusDto
    {
        public string OrderId { get; set; } = null!;
        public OrderStatus NewStatus { get; set; }
    }
}
