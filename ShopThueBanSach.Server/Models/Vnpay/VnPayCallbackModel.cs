namespace ShopThueBanSach.Server.Models.Vnpay
{
	public class VnPayCallbackModel
	{
		public string Vnp_ResponseCode { get; set; }
		public string Vnp_TransactionStatus { get; set; }
		public string Vnp_TxnRef { get; set; }
	}
}