using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Services.Interfaces;

public class ActivityNotificationService : IActivityNotificationService
{
    private readonly AppDBContext _context;

    public ActivityNotificationService(AppDBContext context)
    {
        _context = context;
    }

    public async Task CreateNotificationAsync(string staffId, string description)
    {
        var notification = new ActivityNotification
        {
            NotificationId = Guid.NewGuid().ToString(),
            StaffId = staffId,
            Description = description,
            CreatedDate = DateTime.UtcNow
        };

        _context.ActivityNotifications.Add(notification);
        await _context.SaveChangesAsync();
    }
}
//