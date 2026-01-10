using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Models.SaleModel.SaleOrderModel;
using ShopThueBanSach.Server.Services.Interfaces;
using ShopThueBanSach.Server.Services.Vnpay;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleOrdersController : ControllerBase
    {
		private readonly ISaleOrderService _saleOrderService;
		private readonly IVnPayService _vnPayService;

		public SaleOrdersController(ISaleOrderService saleOrderService, IVnPayService vnPayService)
		{
			_saleOrderService = saleOrderService;
			_vnPayService = vnPayService;
		}

		[Authorize]
        [HttpPost("create-cash")]
        public async Task<IActionResult> CreateOrderCash([FromBody] SaleOrderRequest request)
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
			request.Username = userName;

			if (request.SelectedProductIds == null || !request.SelectedProductIds.Any())
                return BadRequest("Vui lòng chọn ít nhất một sản phẩm để thanh toán.");

            try
            {
                return await _saleOrderService.CreateSaleOrderWithCashAsync(request);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo đơn hàng thanh toán tiền mặt", detail = ex.Message });
            }
        }



    
		[Authorize]
		[HttpPost("create-vnpay")]
		public async Task<IActionResult> CreatePaymentVnPay([FromBody] SaleOrderRequest request)
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
			request.Username = userName;

			try
			{
				var url = await _saleOrderService.PrepareVnPaySaleOrderAsync(request, HttpContext);
				return Ok(new { paymentUrl = url });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "Lỗi tạo đơn hàng VNPAY", detail = ex.Message });
			}
		}



		[HttpGet("PaymentCallbackVnpay")]
		public async Task<IActionResult> PaymentCallbackVnpay()
		{
			var query = Request.Query;
			var vnpResponseCode = query["vnp_ResponseCode"].ToString();
			var vnpTxnStatus = query["vnp_TransactionStatus"].ToString();
			var txnRef = query["vnp_TxnRef"].ToString();

			if (vnpResponseCode == "00" && vnpTxnStatus == "00")
			{
				var result = await _saleOrderService.CreateSaleOrderAfterVnPayAsync(HttpContext);

				if (result is OkObjectResult okResult &&
					okResult.Value is { } value &&
					value.GetType().GetProperty("orderId") is { } prop)
				{
					var orderId = prop.GetValue(value)?.ToString();
					return Redirect($"https://www.shopbookhexa.io.vn/payment-success?orderId={orderId}");
				}

				return Redirect("https://www.shopbookhexa.io.vn/");
			}

			return Redirect("https://www.shopbookhexa.io.vn/");
		}


	}
}

