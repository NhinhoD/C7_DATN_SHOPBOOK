using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class RoleController : ControllerBase
	{
		private readonly IRoleService _roleService;

		public RoleController(IRoleService roleService)
		{
			_roleService = roleService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var roles = await _roleService.GetAllRolesAsync();
			return Ok(roles);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(string id)
		{
			var role = await _roleService.GetByIdAsync(id);
			return role == null ? NotFound() : Ok(role);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] string roleName)
		{
			var success = await _roleService.CreateAsync(roleName);
			return success ? Ok(new { message = "Tạo role thành công." }) :
							 BadRequest(new { message = "Role đã tồn tại." });
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(string id, [FromBody] string newName)
		{
			var success = await _roleService.UpdateAsync(id, newName);
			return success ? Ok(new { message = "Cập nhật role thành công." }) :
							 NotFound(new { message = "Không tìm thấy role." });
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(string id)
		{
			var success = await _roleService.DeleteAsync(id);
			return success ? Ok(new { message = "Xoá role thành công." }) :
							 NotFound(new { message = "Không tìm thấy role." });
		}
	}
}