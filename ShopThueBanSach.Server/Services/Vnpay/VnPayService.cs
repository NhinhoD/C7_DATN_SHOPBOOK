using Newtonsoft.Json;
using ShopThueBanSach.Server.Libraries;
using ShopThueBanSach.Server.Models.SaleModel.CartSaleModel;
using ShopThueBanSach.Server.Models.Vnpay;
using System.Text.Json;
using System.Web;

namespace ShopThueBanSach.Server.Services.Vnpay
{
	public class VnPayService : IVnPayService
	{
		private readonly IConfiguration _configuration;

		public VnPayService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
		{
			var tick = DateTime.UtcNow.Ticks.ToString();
			var txnRef = tick;

			// Lấy giỏ hàng đã chọn
			var cartJson = context.Session.GetString("SaleCart");
			var selectedItems = JsonConvert.DeserializeObject<List<CartItemSale>>(cartJson)?
									.Where(x => true) // bạn có thể lọc selected ở đây nếu cần
									.ToList() ?? new List<CartItemSale>();

			// Tạo thông tin thanh toán để lưu session
			var paymentSession = new PaymentSessionModel
			{
				UserId = model.Name,
				CartItems = selectedItems,
				Amount = model.Amount,
				OrderDescription = model.OrderDescription,
				Tick = tick
			};

			// Lưu vào session
			var sessionKey = $"OrderInfo_{txnRef}";
			var sessionValue = System.Text.Json.JsonSerializer.Serialize(paymentSession);
			context.Session.SetString(sessionKey, sessionValue);

			string frontendRedirectUrl = "https://shopbook-frontend.vercel.app/payment-success";

			// Gắn URL callback server + frontend redirect vào query
			string returnUrlWithRedirect = $"https://c7-datn-shopbook.onrender.com/api/saleorders/PaymentCallbackVnpay?redirect={HttpUtility.UrlEncode(frontendRedirectUrl)}";

			// Tạo URL thanh toán
			var vnpay = new VnPayLibrary();
			vnpay.AddRequestData("vnp_Version", "2.1.0");
			vnpay.AddRequestData("vnp_Command", "pay");
			vnpay.AddRequestData("vnp_TmnCode", "KHPCMS36");
			vnpay.AddRequestData("vnp_Amount", ((int)(model.Amount * 100)).ToString());
			vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
			vnpay.AddRequestData("vnp_CurrCode", "VND");
			vnpay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
			vnpay.AddRequestData("vnp_Locale", "vn");
			vnpay.AddRequestData("vnp_OrderInfo", model.OrderDescription);
			vnpay.AddRequestData("vnp_OrderType", model.OrderType);
			vnpay.AddRequestData("vnp_ReturnUrl", returnUrlWithRedirect);
			vnpay.AddRequestData("vnp_TxnRef", txnRef);

			string paymentUrl = vnpay.CreateRequestUrl("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", "KT3Q7PPNZII0VKFE4Y9TBVELET1L5IYR");
			return paymentUrl;
		}


		public PaymentResponseModel PaymentExecute(IQueryCollection collections)
		{
			var pay = new VnPayLibrary();
			var response = pay.GetFullResponseData(
collections,
				_configuration["Vnpay:HashSecret"]
			);

			return response;
		}




		public string CreatePaymentUrlForRent(PaymentInformationRentModel model, HttpContext context)
		{
			var tick = DateTime.UtcNow.Ticks.ToString();
			var txnRef = tick;

			var paymentSession = new PaymentSessionModel
			{
				UserId = model.UserId,
				Amount = model.Amount,
				OrderDescription = model.OrderDescription,
				RentItems = model.CartItemsRent,
				Tick = tick
			};

			var sessionKey = $"OrderInfo_{txnRef}";
			var sessionValue = System.Text.Json.JsonSerializer.Serialize(paymentSession);
			context.Session.SetString(sessionKey, sessionValue);
			// 127.0.0.1
			var vnpay = new VnPayLibrary();
			vnpay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
			vnpay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
			vnpay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
			vnpay.AddRequestData("vnp_Amount", ((int)(model.Amount * 100)).ToString());
			vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
			vnpay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
			vnpay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
			vnpay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
			vnpay.AddRequestData("vnp_OrderInfo", model.OrderDescription);
			vnpay.AddRequestData("vnp_OrderType", model.OrderType);
			vnpay.AddRequestData("vnp_ReturnUrl", string.IsNullOrEmpty(model.ReturnUrl)
				? _configuration["Vnpay:RentReturnUrl"]
				: model.ReturnUrl);
			vnpay.AddRequestData("vnp_TxnRef", txnRef);

			return vnpay.CreateRequestUrl(
				_configuration["Vnpay:BaseUrl"],
				_configuration["Vnpay:HashSecret"]
			);
		}

		public string GetRentReturnUrl()
		{
			return _configuration["Vnpay:RentReturnUrl"];
		}
	}
}