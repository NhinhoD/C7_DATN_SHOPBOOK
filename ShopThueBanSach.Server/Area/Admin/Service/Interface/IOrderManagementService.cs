using ShopThueBanSach.Server.Area.Admin.Model;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Area.Admin.Service.Interface
{
    public interface IOrderManagementService
    {
        Task<List<RentOrder>> GetAllRentOrdersAsync();
        Task<List<RentOrder>> GetOrdersByStatusAsync(OrderStatus status);
        Task<RentOrder?> GetRentOrderByIdAsync(string orderId);
        Task<List<RentOrderDetailDto>> GetRentOrderDetailDtosAsync(string orderId);
        Task<List<RentOrderDetail>> GetRentOrderDetailsAsync(string orderId);
        Task<bool> UpdateRentOrderStatusAsync(string orderId, OrderStatus status);
        Task<bool> CompleteRentOrderAsync(string orderId, DateTime actualReturnDate, Dictionary<int, int> updatedConditions, Dictionary<int, string> conditionDescriptions);
        Task<int> AutoUpdateOverdueOrdersAsync();
    }
}
