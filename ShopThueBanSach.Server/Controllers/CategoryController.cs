using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Models.BooksModel;
using ShopThueBanSach.Server.Services.Interfaces;
using System.Security.Claims;

namespace ShopThueBanSach.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly IActivityNotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStaffService _staffService;
        private readonly AppDBContext _dbContext;

        public CategoryController(
            ICategoryService service,
            IActivityNotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            IStaffService staffService,
            AppDBContext dbContext)
        {
            _service = service;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _staffService = staffService;
            _dbContext = dbContext;
        }

        /* ------------------ Helper lấy StaffId ------------------ */
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
        /* -------------------------------------------------------- */

        /* ----------------------- CRUD --------------------------- */
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
        public async Task<IActionResult> Create(CategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.CategoryName?.Trim().ToLower() == "string" || dto.Description?.Trim().ToLower() == "string")
            {
                return BadRequest(new { message = "Tên thể loại và mô tả không được để là 'string'." });
            }

            // ❗ Kiểm tra trùng tên
            bool isDuplicate = await _dbContext.Categories
    .AnyAsync(c => c.Name.ToLower() == dto.CategoryName.Trim().ToLower());

            if (isDuplicate)
            {
                return BadRequest(new { message = "Tên thể loại đã tồn tại. Vui lòng nhập tên khác." });
            }

            var result = await _service.CreateAsync(dto);
            await CreateNotificationIfValidAsync($"Thêm thể loại: {dto.CategoryName}");

            return CreatedAtAction(nameof(GetById), new { id = result!.CategoryId }, result);
        }

        // ... các phương thức khác giữ nguyên

        /*[HttpPost]
        public async Task<IActionResult> Create(CategoryDto dto)
        {
            var result = await _service.CreateAsync(dto);

            // 🔔 Tạo thông báo – gán userId tạm thời để kiểm tra
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Nếu không có userId thì gán thủ công để test
            if (string.IsNullOrEmpty(userId))
            {
                userId = "test-user-id"; // 👈 Gán tạm userId (bạn có thể dùng ID thực từ bảng User)
            }

            var description = $"User added new category: {dto.CategoryName}";
            await _notificationService.CreateNotificationAsync(userId, description);
            Console.WriteLine($"[Thông báo] Đã tạo thông báo: {description} (userId = {userId})");

            return CreatedAtAction(nameof(GetById), new { id = result!.CategoryId }, result);
        }
*/

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, CategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ❌ Không được để trống hoặc là "string"
            if (string.IsNullOrWhiteSpace(dto.CategoryName) || string.IsNullOrWhiteSpace(dto.Description) ||
                dto.CategoryName.Trim().ToLower() == "string" || dto.Description.Trim().ToLower() == "string")
            {
                return BadRequest(new { message = "Tên thể loại và mô tả không được để trống hoặc là 'string'." });
            }

            // ❌ Không được trùng tên với category khác (ngoại trừ chính nó)
            bool isDuplicate = await _dbContext.Categories
                .AnyAsync(c => c.Name.ToLower() == dto.CategoryName.Trim().ToLower() && c.CategoryId != id);

            if (isDuplicate)
            {
                return BadRequest(new { message = "Tên thể loại đã tồn tại. Vui lòng nhập tên khác." });
            }

            var result = await _service.UpdateAsync(id, dto);
            if (result == null) return NotFound();

            await CreateNotificationIfValidAsync($"Cập nhật thể loại: {dto.CategoryName}");
            return Ok(result);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();

            await CreateNotificationIfValidAsync($"Xóa thể loại: {id}");
            return NoContent();
        }
    }
}
