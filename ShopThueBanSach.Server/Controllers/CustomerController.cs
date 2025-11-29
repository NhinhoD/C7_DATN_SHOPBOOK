/*using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;

        public CustomerController(ICustomerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("role/{role}")]
        public async Task<IActionResult> GetByRole(string role) => Ok(await _service.GetByRoleAsync(role));

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            var created = await _service.CreateAsync(customer);
            return CreatedAtAction(nameof(GetById), new { id = created.CustomerId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Customer customer)
        {
            if (id != customer.CustomerId) return BadRequest();
            var result = await _service.UpdateAsync(customer);
            return result ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }

        [HttpPatch("ban/{id}")]
        public async Task<IActionResult> Ban(int id)
        {
            var result = await _service.BanAccountAsync(id);
            return result ? Ok(new { message = "Account banned." }) : NotFound();
        }

        [HttpPatch("unban/{id}")]
        public async Task<IActionResult> Unban(int id)
        {
            var result = await _service.UnbanAccountAsync(id);
            return result ? Ok(new { message = "Account unbanned." }) : NotFound();
        }

    }

}
*/