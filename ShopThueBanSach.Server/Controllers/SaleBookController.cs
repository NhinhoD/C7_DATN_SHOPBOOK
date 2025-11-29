using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Models.BooksModel.SaleBooks;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaleBooksController : ControllerBase
    {
        private readonly ISaleBookService _service;
        private readonly IActivityNotificationService _notificationService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SaleBooksController(
            ISaleBookService service,
            IActivityNotificationService notificationService,
            IStaffService staffService,
            IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _notificationService = notificationService;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
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
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }



        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateSaleBookDto dto)

        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Title?.Trim().ToLower() == "string" || dto.Publisher?.Trim().ToLower() == "string")
                return BadRequest(new { message = "Tiêu đề và nhà xuất bản không được để là 'trống'." });

            if (await _service.CheckTitleExistsAsync(dto.Title))
                return BadRequest(new { message = "Tiêu đề sách đã tồn tại." });

            var id = await _service.CreateAsync(dto);
            await CreateNotificationIfValidAsync($"Thêm sách bán: {dto.Title}");

            var result = await _service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, result);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] UpdateSaleBookDto dto)

        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Title?.Trim().ToLower() == "string" || dto.Publisher?.Trim().ToLower() == "string")
                return BadRequest(new { message = "Tiêu đề và nhà xuất bản không được để là 'trống'." });

            if (await _service.CheckTitleExistsAsync(dto.Title, id))
                return BadRequest(new { message = "Tiêu đề sách đã tồn tại." });

            var success = await _service.UpdateAsync(id, dto);
            if (success)
                await CreateNotificationIfValidAsync($"Cập nhật sách bán: {dto.Title}");

            return success ? Ok() : NotFound();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteAsync(id);
            if (success)
                await CreateNotificationIfValidAsync($"Xóa sách bán: {id}");
            return success ? NoContent() : NotFound();
        }

        [HttpPut("set-visibility/{id}/{isHidden}")]
        public async Task<IActionResult> SetVisibility(string id, int isHidden)
        {
            if (isHidden != 0 && isHidden != 1)
                return BadRequest("isHidden must be 0 (false) or 1 (true)");

            var success = await _service.SetVisibilityAsync(id, isHidden == 1);
            if (success)
                await CreateNotificationIfValidAsync($"Cập nhật hiển thị sách bán: {id} -> {(isHidden == 1 ? "ẩn" : "hiện")}");

            return success ? Ok() : NotFound();
        }
    }
}
