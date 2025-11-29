namespace ShopThueBanSach.Server.Models.PaymentMethod
{
    public class MoMoResult
    {
        public string PartnerCode { get; set; }
        public string OrderId { get; set; }
        public string RequestId { get; set; }
        public string Amount { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; }
        public string TransId { get; set; }
        public int ResultCode { get; set; } // 0 = thành công
        public string Message { get; set; }
        public string PayType { get; set; }
        public string ResponseTime { get; set; }
        public string ExtraData { get; set; }
        public string Signature { get; set; }
    }
}
