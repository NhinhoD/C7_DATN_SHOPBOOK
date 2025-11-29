using ShopThueBanSach.Server.Entities;

namespace ShopThueBanSach.Server.Models.SaleModel.SaleOrderModel
{
    public class SaleOrderResult
    {
        public SaleOrder Order { get; set; }
        public List<SaleOrderDetail> Details { get; set; }
        public string? VoucherCode { get; set; }
    }
}
