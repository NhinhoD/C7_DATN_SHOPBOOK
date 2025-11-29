using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Models.CommentModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: api/comment/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllComments()
        {
            var comments = await _commentService.GetAllCommentsAsync();
            return Ok(comments);
        }
			// GET: api/comment/book/{bookId}
			[HttpGet("book/{bookId}")]
        public async Task<IActionResult> GetAll(string bookId)
        {
            var comments = await _commentService.GetAllAsync(bookId);
            return Ok(comments);
        }

        // GET: api/comment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CommentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _commentService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.CommentId }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }


        // DELETE: api/comment/{id}
        [HttpDelete("{id}")]
        [Authorize] // Chỉ user mới được xoá
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _commentService.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
