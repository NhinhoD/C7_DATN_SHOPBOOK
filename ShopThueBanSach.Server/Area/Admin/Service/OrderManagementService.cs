using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Area.Admin.Model;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models;

namespace ShopThueBanSach.Server.Area.Admin.Service
{
    public class OrderManagementService : IOrderManagementService
    {
        private readonly AppDBContext _context;

        public OrderManagementService(AppDBContext context)
        {
            _context = context;
        }

        // Lấy toàn bộ đơn thuê
        public async Task<List<RentOrder>> GetAllRentOrdersAsync()
        {
            return await _context.RentOrders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        //Lấy đơn thuê theo trạng thái (KHÔNG Include User)
        public async Task<List<RentOrder>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.RentOrders
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        //Lấy đơn thuê theo ID (giữ lại thông tin Payment nếu cần)
        public async Task<RentOrder?> GetRentOrderByIdAsync(string orderId)
        {
            return await _context.RentOrders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }
		public async Task<List<RentOrderDetailDto>> GetRentOrderDetailDtosAsync(string orderId)
		{
			return await _context.RentOrderDetails
				.Where(d => d.OrderId == orderId)
				.Select(d => new RentOrderDetailDto
				{
					Id = d.Id,
					BookTitle = d.BookTitle,
					BookPrice = d.BookPrice,
					Condition = d.Condition,
					ConditionDescription = d.ConditionDescription,
					StatusDescription = d.RentBookItem.StatusDescription,
					ReturnCondition = d.ReturnCondition,
					RentalFee = d.RentalFee,
					TotalFee = d.TotalFee,
					ActualRefundAmount = d.ActualRefundAmount,
					ActualReturnDate = d.ActualReturnDate
				})
				.ToListAsync();
		}
		//Lấy chi tiết đơn thuê
		public async Task<List<RentOrderDetail>> GetRentOrderDetailsAsync(string orderId)
        {
            return await _context.RentOrderDetails
                .Where(d => d.OrderId == orderId)
                .Include(d => d.RentBookItem)
                    .ThenInclude(r => r.RentBook)
                .ToListAsync();
        }

		// Cập nhật trạng thái đơn
		public async Task<bool> UpdateRentOrderStatusAsync(string orderId, OrderStatus newStatus)
		{
			var order = await _context.RentOrders.FindAsync(orderId);
			if (order == null) return false;

			// Không cho cập nhật về trạng thái không hợp lệ (nếu cần thêm logic)
			if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
				return false;

			order.Status = newStatus;

			// Nếu hủy đơn => cập nhật RentBookItem về Available
			if (newStatus == OrderStatus.Canceled)
			{
				var orderDetails = await _context.RentOrderDetails
					.Where(d => d.OrderId == orderId)
					.Include(d => d.RentBookItem)
					.ToListAsync();

				foreach (var detail in orderDetails)
				{
					if (detail.RentBookItem != null)
					{
						detail.RentBookItem.Status = RentBookItemStatus.Available;
						detail.RentBookItem.StatusDescription = "Sẵn sàng sau khi hủy đơn";
					}
				}
			}

			await _context.SaveChangesAsync();
			return true;

		}

		//Hoàn tất đơn thuê
		public async Task<bool> CompleteRentOrderAsync(
	string orderId,
	DateTime actualReturnDate,
	Dictionary<int, int> updatedConditions, Dictionary<int, string> conditionDescriptions)
		{
			var order = await _context.RentOrders
				.FirstOrDefaultAsync(o => o.OrderId == orderId);
			if (order == null) return false;

			var details = await _context.RentOrderDetails
				.Where(d => d.OrderId == orderId)
				.Include(d => d.RentBookItem)
				.ToListAsync();
			if (!details.Any()) return false;

			decimal totalRefund = 0;
			int lateDays = (actualReturnDate - order.EndDate).Days;
			lateDays = lateDays > 0 ? lateDays : 0;

			foreach (var detail in details)
			{
				if (!updatedConditions.TryGetValue(detail.Id, out int returnedCondition))
					continue;

				detail.ActualReturnDate = actualReturnDate;
				detail.ReturnCondition = returnedCondition;

				if (conditionDescriptions.TryGetValue(detail.Id, out string description))
				{
					detail.ConditionDescription = description;
				}
				else
				{
					detail.ConditionDescription = string.Empty;
				}

				int lostCondition = detail.Condition - returnedCondition;
				if (lostCondition < 0) lostCondition = 0;

				decimal lateFee = lateDays * 3000;
				decimal damageFee = (lostCondition / 100m) * detail.BookPrice;
				decimal totalPenalty = lateFee + damageFee;

				bool forfeitDeposit = lateDays > 60 || lostCondition > 40;
				decimal refund = forfeitDeposit ? 0 : Math.Max(detail.BookPrice - totalPenalty, 0);

				detail.ActualRefundAmount = refund;
				totalRefund += refund;

				if (detail.RentBookItem != null)
				{
					detail.RentBookItem.Status = RentBookItemStatus.Available;
					detail.RentBookItem.Condition = returnedCondition;
				}
			}

			order.Status = OrderStatus.Completed;
			order.ActualReturnDate = actualReturnDate;
			order.ActualRefundAmount = totalRefund;


			await _context.SaveChangesAsync();
			return true;
		}


		//Tự động cập nhật đơn quá hạn
		public async Task<int> AutoUpdateOverdueOrdersAsync()
		{
			var today = DateTime.UtcNow;

			var overdueOrders = await _context.RentOrders
				.Where(o => o.Status != OrderStatus.Pending
						 && o.Status != OrderStatus.Confirmed
						 && o.Status != OrderStatus.Canceled
						 && o.Status != OrderStatus.Completed
						 && o.EndDate < today)
				.ToListAsync();

			int updatedCount = 0;

			foreach (var order in overdueOrders)
			{
				bool updated = await UpdateRentOrderStatusAsync(order.OrderId, OrderStatus.Overdue);
				if (updated) updatedCount++;
			}

			return updatedCount;
		}

	}
}
