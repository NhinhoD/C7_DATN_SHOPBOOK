using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace ShopThueBanSach.Server.Services
{
    public class MoMoPaymentService : IMoMoPaymentService
    {
        private readonly IConfiguration _configuration;

        public MoMoPaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> CreatePaymentUrlAsync(string orderId, decimal amount, string returnUrl, string notifyUrl, string extraData)
        {
            var endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
            var partnerCode = _configuration["MoMo:PartnerCode"];
            var accessKey = _configuration["MoMo:AccessKey"];
            var secretKey = _configuration["MoMo:SecretKey"];
            var requestId = Guid.NewGuid().ToString("N");
            var orderInfo = $"Thanh toán đơn thuê sách {orderId}";
            var requestType = "captureWallet";
            var amountStr = ((int)amount).ToString();

            // Ensure extraData is not null
            extraData ??= "";

            // Raw hash string must match MoMo spec
            var rawHash = $"accessKey={accessKey}&amount={amountStr}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestType}";
            var signature = HmacSHA256(rawHash, secretKey);

            var payload = new
            {
                partnerCode,
                accessKey,
                requestId,
                amount = amountStr,
                orderId,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                extraData,
                requestType,
                signature,
                lang = "vi"
            };

            using var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"MoMo API request failed: {response.StatusCode} - {responseContent}");

            using var jsonDoc = JsonDocument.Parse(responseContent);
            if (jsonDoc.RootElement.TryGetProperty("payUrl", out var payUrlElement))
            {
                return payUrlElement.GetString() ?? throw new Exception("Không tìm thấy payUrl từ MoMo");
            }

            throw new Exception("Phản hồi MoMo không hợp lệ: thiếu 'payUrl'");
        }

        private string HmacSHA256(string rawData, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
