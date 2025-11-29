namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IActivityNotificationService
    {
        Task CreateNotificationAsync(string staffId, string description);
    }

}
