using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class ActivityNotificationController : ControllerBase
    {
        private readonly IActivityNotificationService _notificationService;
        private readonly AppDBContext _context;
        // ✅ Thêm AppDBContext vào constructor
        public ActivityNotificationController(
            IActivityNotificationService notificationService,
            AppDBContext context)
        {
            _notificationService = notificationService;
            _context = context;
        }

        // GET: api/ActivityNotification
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = HttpContext.User;

            if (user == null || !user.Identity.IsAuthenticated)
                return Unauthorized();

            var userId = user.FindFirst("UserId")?.Value;
            var isAdmin = user.IsInRole("Admin");

            var query = _context.ActivityNotifications
                .Include(n => n.Staff)
                .AsQueryable();

            if (!isAdmin)
            {
                // Nếu không phải admin thì chỉ xem được thông báo của chính mình
                query = query.Where(n => n.StaffId == userId);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Description,
                    n.CreatedDate,
                    StaffId = n.StaffId,
                    StaffName = n.Staff != null ? n.Staff.FullName : "Unknown"
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // GET: api/ActivityNotification/{id}
        /*  [HttpGet("{id}")]
          public async Task<IActionResult> GetById(string id)
          {
              var notification = await _context.ActivityNotifications
                  .Include(n => n.Staff)
                  .FirstOrDefaultAsync(n => n.NotificationId == id);

              if (notification == null)
                  return NotFound();

              return Ok(new
              {
                  notification.NotificationId,
                  notification.Description,
                  notification.CreatedDate,
                  StaffId = notification.StaffId,
                  StaffName = notification.Staff?.FullName ?? "Unknown"
              });
          }*/
        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetByStaffId(string staffId)
        {
            var notifications = await _context.ActivityNotifications
                .Where(n => n.StaffId == staffId)
                .Include(n => n.Staff)
                .OrderByDescending(n => n.CreatedDate)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Description,
                    n.CreatedDate,
                    StaffId = n.StaffId,
                    StaffName = n.Staff != null ? n.Staff.FullName : "Unknown"
                })
                .ToListAsync();

            return Ok(notifications);
        }



    }
}
