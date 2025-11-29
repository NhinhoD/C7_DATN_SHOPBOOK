using ShopThueBanSach.Server.Entities;

namespace ShopThueBanSach.Server.Models.RentOrderModel
{
    public class RentOrderResult
    {
        public RentOrder Order { get; set; }
        public List<RentOrderDetail> Details { get; set; }
    }
}
