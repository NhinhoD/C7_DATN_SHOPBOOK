using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Models;
using System.Threading.Tasks;

namespace ShopThueBanSach.Server.Area.Admin.Controllers
{
    [Route("api/admin/saleorders")]
    [ApiController]
 
    public class SaleOrdersManagementController : ControllerBase
    {
        private readonly ISaleOrderManagementService _saleOrderService;

        public SaleOrdersManagementController(ISaleOrderManagementService saleOrderService)
        {
            _saleOrderService = saleOrderService;
        }

        // GET: api/admin/saleorders
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _saleOrderService.GetAllSaleOrdersAsync();
            return Ok(orders);
        }

        // GET: api/admin/saleorders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var order = await _saleOrderService.GetSaleOrderByIdAsync(id);
            if (order == null) return NotFound("Không tìm thấy đơn hàng.");
            return Ok(order);
        }

        // GET: api/admin/saleorders/{id}/details
        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetDetails(string id)
        {
            var details = await _saleOrderService.GetSaleOrderDetailsAsync(id);
            return Ok(details);
        }

        // GET: api/admin/saleorders/status/{status}
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(OrderStatus status)
        {
            var orders = await _saleOrderService.GetSaleOrdersByStatusAsync(status);
            return Ok(orders);
        }

        // PUT: api/admin/saleorders/{id}/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] OrderStatus status)
        {
            var success = await _saleOrderService.UpdateSaleOrderStatusAsync(id, status);
            if (!success) return NotFound("Không thể cập nhật trạng thái.");
            return Ok("Cập nhật trạng thái thành công.");
        }
    }
}
