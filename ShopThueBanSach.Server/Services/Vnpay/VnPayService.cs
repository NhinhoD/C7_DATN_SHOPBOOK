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

            // 1. Lưu giỏ hàng vào Session (Giữ nguyên logic của bạn)
            var cartJson = context.Session.GetString("SaleCart");
            var selectedItems = JsonConvert.DeserializeObject<List<CartItemSale>>(cartJson)?
                                    .Where(x => true)
                                    .ToList() ?? new List<CartItemSale>();

            var paymentSession = new PaymentSessionModel
            {
                UserId = model.Name,
                CartItems = selectedItems,
                Amount = model.Amount,
                OrderDescription = model.OrderDescription,
                Tick = tick
            };

            var sessionKey = $"OrderInfo_{txnRef}";
            var sessionValue = System.Text.Json.JsonSerializer.Serialize(paymentSession);
            context.Session.SetString(sessionKey, sessionValue);

            // 2. TẠO URL TRẢ VỀ CHUẨN (Không kèm ?redirect=...)
            // URL này PHẢI GIỐNG Y HỆT URL bạn điền trong trang quản trị VNPAY mục "Return URL"
            string vnp_ReturnUrl = "https://c7-datn-shopbook.onrender.com/api/saleorders/PaymentCallbackVnpay";

            // 3. Cấu hình VNPAY Library
            var vnpay = new VnPayLibrary();

            // Nên lấy từ Config thay vì Hard-code (để dễ sửa trên Render)
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", "JKAVQQQ3"); // Đảm bảo đúng mã Terminal

            vnpay.AddRequestData("vnp_Amount", ((int)(model.Amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");

            // QUAN TRỌNG: GÁN CỨNG IP ĐỂ TRÁNH LỖI IPV6 (::1)
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");

            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", model.OrderDescription);
            vnpay.AddRequestData("vnp_OrderType", "other"); // Thường sandbox dùng "other" hoặc "billpayment" cho an toàn
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", txnRef);

            // HashSecret lấy từ Config hoặc điền trực tiếp nếu bạn muốn test nhanh
            string paymentUrl = vnpay.CreateRequestUrl(
                "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
                _configuration["Vnpay:HashSecret"] // Đảm bảo biến này trên Render đúng HashSecret Sandbox
            );

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

        // ... (Giữ nguyên phần Rent nếu cần, nhưng nhớ sửa vnp_IpAddr thành "127.0.0.1" tương tự)
        public string CreatePaymentUrlForRent(PaymentInformationRentModel model, HttpContext context)
        {
            // ... (Logic session giữ nguyên) ...

            var vnpay = new VnPayLibrary();
            // ... các thông số khác ...

            // SỬA DÒNG NÀY:
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");

            // ...
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