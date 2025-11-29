using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Area.Admin.Controllers
{
    [Route("api/admin/rentorders")]
    [ApiController]
   
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderManagementService _orderService;

        public AdminOrdersController(IOrderManagementService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/admin/rentorders
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllRentOrdersAsync();
            return Ok(orders);
        }

        // GET: api/admin/rentorders/status/Pending
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(OrderStatus status)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(orders);
        }

        // GET: api/admin/rentorders/{orderId}
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            var order = await _orderService.GetRentOrderByIdAsync(orderId);
            return order == null ? NotFound("Không tìm thấy đơn hàng.") : Ok(order);
        }

        // GET: api/admin/rentorders/{orderId}/details
        [HttpGet("{orderId}/details")]
        public async Task<IActionResult> GetOrderDetails(string orderId)
        {
            var details = await _orderService.GetRentOrderDetailDtosAsync(orderId);
            return Ok(details);
        }

        // PUT: api/admin/rentorders/{orderId}/status
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(string orderId, [FromBody] OrderStatus newStatus)
        {
            var result = await _orderService.UpdateRentOrderStatusAsync(orderId, newStatus);
            return result ? Ok("Cập nhật trạng thái thành công.") : NotFound("Không tìm thấy đơn hàng.");
        }

		// PUT: api/admin/rentorders/{orderId}/complete
		[HttpPut("{orderId}/complete")]
		public async Task<IActionResult> CompleteOrder(
  string orderId,
  [FromBody] RentOrderCompleteRequest request)
		{
			var result = await _orderService.CompleteRentOrderAsync(
				orderId,
				request.ActualReturnDate,
				request.UpdatedConditions,
				request.ConditionDescriptions
			);

			return result ? Ok("Đơn hàng đã hoàn tất.") : BadRequest("Xử lý thất bại.");
		}


		// PUT: api/admin/rentorders/auto-overdue
		[HttpPut("auto-overdue")]
		public async Task<IActionResult> UpdateOverdueOrders()
		{
			var updatedCount = await _orderService.AutoUpdateOverdueOrdersAsync();
			return Ok(new { message = "Cập nhật thành công", totalUpdated = updatedCount });
		}
	}

    // DTO cho hoàn tất đơn hàng
    public class RentOrderCompleteRequest
    {
        public DateTime ActualReturnDate { get; set; }

        // key = RentOrderDetail.Id, value = tình trạng sách khi trả
        public Dictionary<int, int> UpdatedConditions { get; set; } = new();
		public Dictionary<int, string> ConditionDescriptions { get; set; }
	}
}
