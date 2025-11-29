using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Models.SlideModel;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlideController : ControllerBase
    {
        private readonly ISlideService _service;
        private readonly IActivityNotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStaffService _staffService;

        public SlideController(
            ISlideService service,
            IActivityNotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            IStaffService staffService)
        {
            _service = service;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _staffService = staffService;
        }

        private async Task<string?> GetCurrentStaffIdAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && await _staffService.ExistsAsync(userId))
                return userId;
            return null;
        }

        private async Task CreateNotificationIfValidAsync(string description)
        {
            var staffId = await GetCurrentStaffIdAsync();
            if (!string.IsNullOrEmpty(staffId))
            {
                await _notificationService.CreateNotificationAsync(staffId, description);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var slide = await _service.GetByIdAsync(id);
            if (slide == null) return NotFound();
            return Ok(slide);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] SlideDto dto)
        {
            var result = await _service.CreateAsync(dto);
            await CreateNotificationIfValidAsync($"Thêm slide mới {(dto.LinkUrl != null ? $"với link: {dto.LinkUrl}" : "")}");
            return CreatedAtAction(nameof(GetById), new { id = result.SlideId }, result);
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> Update(string id, [FromForm] SlideDto dto)
        //{
        //    var result = await _service.UpdateAsync(id, dto);
        //    if (result == null) return NotFound();

        //    await CreateNotificationIfValidAsync($"Cập nhật slide ID: {id}");
        //    return Ok(result);
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();

            await CreateNotificationIfValidAsync($"Xóa slide ID: {id}");
            return NoContent();
        }
    }
}
