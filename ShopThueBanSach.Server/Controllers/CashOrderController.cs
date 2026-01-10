using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Models.RentOrderModel;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CashOrderController : ControllerBase
	{
		private readonly IRentOrderService _rentOrderService;

		public CashOrderController(IRentOrderService rentOrderService)
		{
			_rentOrderService = rentOrderService;
		}
		[Authorize]
		[HttpPost("create")]
		public async Task<IActionResult> CreateCashOrder([FromBody] RentOrderRequest request)
		{
			var user = User;

			var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
					  ?? user.FindFirstValue("sub");

			var userName = user.FindFirstValue(ClaimTypes.Name)
						 ?? user.FindFirstValue("name")
						 ?? user.FindFirstValue("unique_name");

			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
				return Unauthorized("Không xác định được thông tin người dùng.");

			// Gán thông tin người dùng vào request trước khi gọi service
			request.UserId = userId;
			request.UserName = userName;
			return await _rentOrderService.CreateRentOrderWithCashAsync(request);
		}



		[Authorize]
		[HttpPost("create-vnpay")]
		public async Task<IActionResult> CreatePaymentVnPay([FromBody] RentOrderRequest request)
		{
			var user = User;

			var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)
					  ?? user.FindFirstValue("sub");

			var userName = user.FindFirstValue(ClaimTypes.Name)
						 ?? user.FindFirstValue("name")
						 ?? user.FindFirstValue("unique_name");

			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
				return Unauthorized("Không xác định được thông tin người dùng.");

			// Gán thông tin người dùng vào request trước khi gọi service
			request.UserId = userId;
			request.UserName = userName;

			try
			{
				var result = await _rentOrderService.PrepareVnPayRentOrderAsync(request);

				if (result is OkObjectResult okResult && okResult.Value != null)
				{
					var dynamicValue = (dynamic)okResult.Value;
					return Ok(new { success = true, paymentUrl = dynamicValue.PaymentUrl });
				}

				return result;
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					success = false,
					message = "Lỗi tạo đơn thuê qua VNPAY.",
					detail = ex.Message
				});
			}
		}

		/// <summary>
		/// Xử lý callback từ VNPAY sau khi thanh toán
		/// </summary>
		[HttpGet("payment-callback-vnpay")]
		public async Task<IActionResult> PaymentCallbackVnpay()
		{
			var query = Request.Query;

			var vnpResponseCode = query["vnp_ResponseCode"].ToString();
			var vnpTxnStatus = query["vnp_TransactionStatus"].ToString();
			var txnRef = query["vnp_TxnRef"].ToString();

			// Kiểm tra mã thành công từ VNPAY
			if (vnpResponseCode == "00" && vnpTxnStatus == "00")
			{
				try
				{
					var result = await _rentOrderService.CreateRentOrderAfterVnPayAsync(HttpContext);

					if (result is OkObjectResult ok && ok.Value is not null)
					{
						// Lấy OrderId để chuyển hướng
						var orderObject = ok.Value;
						var orderIdProperty = orderObject.GetType().GetProperty("OrderId");
						var orderId = orderIdProperty?.GetValue(orderObject)?.ToString();

						// ✅ Redirect về frontend
						return Redirect($"https://www.shopbookhexa.io.vn/payment-success?status=success&orderId={orderId}");
					}

					return StatusCode(500, new
					{
						success = false,
						message = "Đã thanh toán nhưng không tạo được đơn hàng."
					});
				}
				catch (Exception ex)
				{
					return StatusCode(500, new
					{
						success = false,
						message = "Lỗi xử lý đơn thuê sau thanh toán",
						detail = ex.Message
					});
				}
			}

			// ❌ Trường hợp thanh toán thất bại
			return Redirect($"https://www.shopbookhexa.io.vn/payment-success?status=fail&orderId={txnRef}");
		}

	}
}
