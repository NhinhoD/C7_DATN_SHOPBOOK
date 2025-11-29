using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Models.BooksModel.RentBooks;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentBooksController : ControllerBase
    {
        private readonly IRentBookService _service;
        private readonly IActivityNotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStaffService _staffService;

        public RentBooksController(
            IRentBookService service,
            IActivityNotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            IStaffService staffService)
        {
            _service = service;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _staffService = staffService;
        }
        // 🔍 Lấy StaffId từ JWT Claims
        private async Task<string?> GetCurrentStaffIdAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var exists = await _staffService.ExistsAsync(userId);
                if (exists)
                {
                    return userId;
                }
            }
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
            var rentBook = await _service.GetByIdAsync(id);
            return rentBook == null ? NotFound() : Ok(rentBook);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateRentBookDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Title?.Trim().ToLower() == "string" || dto.Publisher?.Trim().ToLower() == "string")
                return BadRequest(new { message = "Tiêu đề và nhà xuất bản không hợp lệ." });

            if (await _service.CheckTitleExistsAsync(dto.Title))
                return BadRequest(new { message = "Tiêu đề sách thuê đã tồn tại." });

            var id = await _service.CreateAsync(dto, dto.ImageFile); // ✅ pass ảnh
            await CreateNotificationIfValidAsync($"Thêm sách thuê: {dto.Title}");
            return CreatedAtAction(nameof(GetById), new { id }, dto);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] UpdateRentBookDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _service.CheckTitleExistsAsync(dto.Title, id))
                return BadRequest(new { message = "Tiêu đề sách thuê đã tồn tại." });

            var result = await _service.UpdateAsync(id, dto);
            if (!result) return NotFound();

            await CreateNotificationIfValidAsync($"Cập nhật sách thuê: {dto.Title}");
            return Ok(new { message = "Cập nhật sách thành công." });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _service.DeleteAsync(id);
            if (result)
                await CreateNotificationIfValidAsync($"Xóa sách thuê: {id}");
            return result ? NoContent() : NotFound();
        }

        [HttpPut("set-visibility/{id}/{isHidden}")]
        public async Task<IActionResult> SetVisibility(string id, int isHidden)
        {
            var result = await _service.SetVisibilityAsync(id, isHidden == 1);
            if (result)
                await CreateNotificationIfValidAsync($"Cập nhật hiển thị sách thuê: {id} -> {(isHidden == 1 ? "ẩn" : "hiện")}");
            return result ? NoContent() : NotFound();
        }
    }
}
