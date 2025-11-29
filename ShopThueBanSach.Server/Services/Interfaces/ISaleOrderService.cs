using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Models.SaleModel.SaleOrderModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface ISaleOrderService
    {
        // Đơn hàng thanh toán bằng tiền mặt
        Task<IActionResult> CreateSaleOrderWithCashAsync(SaleOrderRequest request);

        // Chuẩn bị đơn hàng để thanh toán qua MoMo
      
		Task<string> PrepareVnPaySaleOrderAsync(SaleOrderRequest request, HttpContext httpContext);

		Task<IActionResult> CreateSaleOrderAfterVnPayAsync(HttpContext httpContext);
	}
}
