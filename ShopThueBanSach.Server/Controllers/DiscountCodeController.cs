using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Models.BooksModel.DiscountCode;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiscountCodeController : ControllerBase
    {
        private readonly IDiscountCodeService _service;

        public DiscountCodeController(IDiscountCodeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDiscountCodeDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.DiscountCodeName?.Trim().ToLower() == "string" ||
                model.Description?.Trim().ToLower() == "string")
            {
                return BadRequest(new { message = "Tên mã và mô tả không hợp lệ." });
            }

            if (model.EndDate <= model.StartDate)
            {
                return BadRequest(new { message = "Ngày kết thúc phải sau ngày bắt đầu." });
            }

            var result = await _service.CreateAsync(model);
            return result ? Ok() : BadRequest(new { message = "Tạo mã giảm giá thất bại." });
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, UpdateDiscountCodeDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.StartDate.HasValue && model.EndDate.HasValue &&
                model.EndDate.Value <= model.StartDate.Value)
            {
                return BadRequest(new { message = "Ngày kết thúc phải sau ngày bắt đầu." });
            }

            var result = await _service.UpdateAsync(id, model);
            return result ? Ok() : NotFound();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? Ok() : NotFound();
        }
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableForExchange()
        {
            var result = await _service.GetAvailableForExchangeAsync();
            return Ok(result);
        }

    }
}
