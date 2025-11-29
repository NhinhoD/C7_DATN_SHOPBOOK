using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Models.BooksModel;
using ShopThueBanSach.Server.Models.BooksModel.Promotion;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _service;

        public PromotionController(IPromotionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllPromotionsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetPromotionByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PromotionDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.PromotionName?.Trim().ToLower() == "string")
                return BadRequest(new { message = "Tên khuyến mãi không hợp lệ." });

            if (model.DiscountPercentage <= 0 || model.DiscountPercentage > 100)
                return BadRequest(new { message = "Phần trăm giảm giá phải lớn hơn 0 và nhỏ hơn hoặc bằng 100." });

            var isDuplicate = await _service.CheckNameExistsAsync(model.PromotionName);
            if (isDuplicate)
                return BadRequest(new { message = "Tên khuyến mãi đã tồn tại." });

            var result = await _service.CreatePromotionAsync(model);
            return result ? Ok() : BadRequest(new { message = "Tạo khuyến mãi thất bại." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] PromotionDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.PromotionName?.Trim().ToLower() == "string")
                return BadRequest(new { message = "Tên khuyến mãi không hợp lệ." });

            if (model.DiscountPercentage <= 0 || model.DiscountPercentage > 100)
                return BadRequest(new { message = "Phần trăm giảm giá phải lớn hơn 0 và nhỏ hơn hoặc bằng 100." });

            var isDuplicate = await _service.CheckNameExistsAsync(model.PromotionName, id);
            if (isDuplicate)
                return BadRequest(new { message = "Tên khuyến mãi đã tồn tại." });

            var result = await _service.UpdatePromotionAsync(id, model);
            return result ? Ok() : NotFound(new { message = "Không tìm thấy khuyến mãi cần cập nhật." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _service.DeletePromotionAsync(id);
            return result ? Ok() : NotFound(new { message = "Không tìm thấy khuyến mãi cần xoá." });
        }

        //[HttpPost("apply")]
        //public async Task<IActionResult> ApplyPromotionToBooks([FromBody] ApplyPromotionDTO dto)
        //{
        //    if (dto == null || string.IsNullOrWhiteSpace(dto.PromotionId) || dto.SaleBookIds == null || !dto.SaleBookIds.Any())
        //        return BadRequest(new { message = "Vui lòng cung cấp đầy đủ PromotionId và danh sách SaleBookIds." });

        //    var result = await _service.ApplyPromotionToBooksAsync(dto);
        //    return result
        //        ? Ok(new { message = "Áp dụng khuyến mãi thành công!" })
        //        : BadRequest(new { message = "Không thể áp dụng khuyến mãi." });
        //}
    }
}
