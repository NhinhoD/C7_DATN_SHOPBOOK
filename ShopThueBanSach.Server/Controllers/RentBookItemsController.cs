using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Models.BooksModel;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RentBookItemController : ControllerBase
    {
        private readonly IRentBookItemService _rentBookItemService;
        private readonly IActivityNotificationService _notificationService;
        private readonly IStaffService _staffService;

        public RentBookItemController(
            IRentBookItemService rentBookItemService,
            IActivityNotificationService notificationService,
            IStaffService staffService)
        {
            _rentBookItemService = rentBookItemService;
            _notificationService = notificationService;
            _staffService = staffService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _rentBookItemService.GetAllAsync();
            return Ok(list.Select(x => new
            {
                x.RentBookItemId,
                x.RentBookId,
                x.RentBookTitle,
                status = x.Status.ToString(),
		x.StatusDescription,
				x.Condition,
                x.IsHidden
            }));
        }

        // ✅ Đổi từ int? sang string?
        private string? GetCurrentStaffId()
        {
            var claim = User.FindFirst("StaffId")?.Value;
            return !string.IsNullOrEmpty(claim) ? claim : null;
        }

        private async Task CreateNotificationIfStaffExistsAsync(string description)
        {
            var staffId = GetCurrentStaffId();
            if (!string.IsNullOrEmpty(staffId) && await _staffService.ExistsAsync(staffId))
            {
                await _notificationService.CreateNotificationAsync(staffId, description);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var item = await _rentBookItemService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return Ok(new
            {
                item.RentBookItemId,
                item.RentBookId,
                item.RentBookTitle,
                status = item.Status.ToString(),
				item.StatusDescription,
				item.Condition,
                item.IsHidden
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RentBookItemDto dto)
        {
            dto.RentBookItemId = null;

            try
            {
                var created = await _rentBookItemService.CreateAsync(dto);
                if (created == null)
                    return BadRequest("Điều kiện phải từ 80 đến 100.");

                await CreateNotificationIfStaffExistsAsync($"Thêm bản sao sách thuê cho sách: {dto.RentBookId}");
                return CreatedAtAction(nameof(GetById), new { id = created.RentBookItemId }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] RentBookItemDto dto)
        {
            var updated = await _rentBookItemService.UpdateAsync(id, dto);
            if (updated == null)
                return BadRequest("Cập nhật thất bại. Kiểm tra ID hoặc điều kiện.");

            await CreateNotificationIfStaffExistsAsync($"Cập nhật bản sao sách thuê: {id}");
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _rentBookItemService.DeleteAsync(id);
            if (success)
                await CreateNotificationIfStaffExistsAsync($"Xóa bản sao sách thuê: {id}");

            return success ? NoContent() : NotFound();
        }
    }
}
