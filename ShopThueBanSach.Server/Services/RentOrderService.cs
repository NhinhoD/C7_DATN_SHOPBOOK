using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models;
using ShopThueBanSach.Server.Models.CartRentModel;
using ShopThueBanSach.Server.Models.RentOrderModel;
using ShopThueBanSach.Server.Models.Vnpay;
using ShopThueBanSach.Server.Services.Interfaces;
using ShopThueBanSach.Server.Services.Vnpay;
using System.Text.Json;

namespace ShopThueBanSach.Server.Services
{
    public class RentOrderService : IRentOrderService
    {
		private readonly AppDBContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IMoMoPaymentService _moMoPaymentService;
		private readonly IVnPayService _vnPayService;

		public RentOrderService(AppDBContext context, IHttpContextAccessor httpContextAccessor, IMoMoPaymentService momo, IVnPayService vnPayService)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_moMoPaymentService = momo;
			_vnPayService = vnPayService;
		}

		// [1] Tạo URL thanh toán VNPAY
		public async Task<IActionResult> PrepareVnPayRentOrderAsync(RentOrderRequest request)
		{
			var result = await BuildOrderFromSessionCartAsync(request);
			if (result is IActionResult error) return error;

			var orderData = (RentOrderResult)result;

			var paymentModel = new PaymentInformationRentModel
			{
				OrderType = "rent",
				Amount = orderData.Order.TotalFee,
				OrderDescription = $"Thanh toán đơn thuê sách lúc {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
				Name = orderData.Order.UserId,
				UserId = orderData.Order.UserId,
				ReturnUrl = _vnPayService.GetRentReturnUrl(), // bạn có thể tạo hàm GetRentReturnUrl() trả về từ config
				CartItemsRent = orderData.Details.Select(x => new CartItemRent
				{
					RentBookItemId = x.RentBookItemId,
					RentBookTitle = x.BookTitle,
					BookPrice = x.BookPrice,
					RentalFee = x.RentalFee,
					TotalFee = x.TotalFee,
					Condition = x.Condition
				}).ToList()
			};

			var sessionData = new PaymentSessionModel
			{
				UserId = orderData.Order.UserId,
                UserName = orderData.Order.UserName,
				Amount = orderData.Order.TotalFee,
				OrderDescription = paymentModel.OrderDescription,
				RentItems = paymentModel.CartItemsRent,
				StartDate = request.StartDate,
				EndDate = request.EndDate,
				Address = request.Address,
				Phone = request.Phone,
				HasShippingFee = request.HasShippingFee
			};

			_httpContextAccessor.HttpContext!.Session.SetString("VNPayRentSession", JsonSerializer.Serialize(sessionData));

			var url = _vnPayService.CreatePaymentUrlForRent(paymentModel, _httpContextAccessor.HttpContext);
			return new OkObjectResult(new { PaymentUrl = url });
		}

		// [2] Tạo đơn sau khi thanh toán VNPAY thành công
		public async Task<IActionResult> CreateRentOrderAfterVnPayAsync(HttpContext context)
		{
			var session = context.Session;
			var json = session.GetString("VNPayRentSession");

			if (string.IsNullOrEmpty(json))
				return new BadRequestObjectResult(new { success = false, message = "Không tìm thấy dữ liệu thanh toán trong session." });

			var sessionData = JsonSerializer.Deserialize<PaymentSessionModel>(json);
			if (sessionData == null)
				return new BadRequestObjectResult(new { success = false, message = "Dữ liệu session không hợp lệ." });

			var rentalDays = (sessionData.EndDate - sessionData.StartDate).Days;
			if (rentalDays <= 0)
				return new BadRequestObjectResult(new { success = false, message = "Ngày thuê không hợp lệ." });

			const decimal shippingFee = 20000;
			var orderId = Guid.NewGuid().ToString();
			var orderDetails = new List<RentOrderDetail>();
			decimal totalDeposit = 0;

			foreach (var item in sessionData.RentItems)
			{
				var rentBookItem = await _context.RentBookItems
					.Include(r => r.RentBook)
					.FirstOrDefaultAsync(x => x.RentBookItemId == item.RentBookItemId);

				if (rentBookItem == null || rentBookItem.RentBook == null || rentBookItem.Status != RentBookItemStatus.Available)
					continue;

				totalDeposit += rentBookItem.RentBook.Price;

				orderDetails.Add(new RentOrderDetail
				{
					OrderId = orderId,
					RentBookItemId = rentBookItem.RentBookItemId,
					BookTitle = rentBookItem.RentBook.Title,
					BookPrice = rentBookItem.RentBook.Price,
					RentalFee = item.RentalFee,
					TotalFee = item.TotalFee,
					Condition = item.Condition
				});

				rentBookItem.Status = RentBookItemStatus.Rented;
			}

			if (!orderDetails.Any())
				return new BadRequestObjectResult(new { success = false, message = "Không có sách hợp lệ để tạo đơn hàng." });

			var order = new RentOrder
			{
				OrderId = orderId,
				UserId = sessionData.UserId,
                UserName = sessionData.UserName,
                Address = sessionData.Address,
                Phone = sessionData.Phone,
				StartDate = sessionData.StartDate,
				EndDate = sessionData.EndDate,
				RentalDays = rentalDays,
				HasShippingFee = sessionData.HasShippingFee,
				ShippingFee = sessionData.HasShippingFee ? shippingFee : 0,
				TotalDeposit = totalDeposit,
				TotalFee = sessionData.Amount,
				OrderDate = DateTime.UtcNow,
				Status = OrderStatus.Pending,
				Payment = "VNPAY"
			};

			await _context.RentOrders.AddAsync(order);
			await _context.RentOrderDetails.AddRangeAsync(orderDetails);
			await _context.SaveChangesAsync();

			session.Remove("VNPayRentSession");

			return new OkObjectResult(new
			{
				success = true,
				message = "Đơn thuê đã được tạo thành công sau khi thanh toán.",
				order = new
				{
					order.OrderId,
					order.TotalFee,
					order.RentalDays,
					order.ShippingFee,
					order.TotalDeposit,
					order.StartDate,
					order.EndDate
				}
			});
		}
		public async Task<IActionResult> CreateRentOrderWithCashAsync(RentOrderRequest request)
        {
            var result = await BuildOrderFromSessionCartAsync(request);
            if (result is IActionResult error) return error;

            var orderData = (RentOrderResult)result;
            await SaveOrderAsync(orderData);

            return new OkObjectResult(new
            {
                Message = "Tạo đơn hàng thành công",
                OrderId = orderData.Order.OrderId,
                TotalAmount = orderData.Order.TotalFee
            });
        }

        public async Task<IActionResult> PrepareMoMoOrderAsync(RentOrderRequest request)
        {
            var result = await BuildOrderFromSessionCartAsync(request);
            if (result is IActionResult error) return error;

            var orderData = (RentOrderResult)result;

            var extraData = JsonSerializer.Serialize(request);

            string payUrl = await _moMoPaymentService.CreatePaymentUrlAsync(
                orderId: orderData.Order.OrderId,
                amount: orderData.Order.TotalFee,
                returnUrl: "https://chosachonline-datn.onrender.com/momo-return",
                notifyUrl: "https://chosachonline-datn.onrender.com/api/momoorder/callback",
                extraData: extraData
            );

            return new OkObjectResult(new
            {
                Message = "Chuyển đến thanh toán MoMo",
                PaymentUrl = payUrl
            });
        }

        public async Task CreateRentOrderAfterMoMoAsync(RentOrderRequest request)
        {
            var result = await BuildOrderFromSessionCartAsync(request);
            if (result is not RentOrderResult orderData) return;

            await SaveOrderAsync(orderData);
        }

        private async Task<object> BuildOrderFromSessionCartAsync(RentOrderRequest request)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return new BadRequestObjectResult("No HttpContext");

            var session = context.Session;
            var json = session.GetString("RentalCart");
            if (string.IsNullOrEmpty(json)) return new BadRequestObjectResult("Giỏ hàng trống");

            var cart = JsonSerializer.Deserialize<List<CartItemRent>>(json);
            if (cart == null || !cart.Any()) return new BadRequestObjectResult("Không có sản phẩm trong giỏ");

            int rentalDays = (request.EndDate - request.StartDate).Days;
            if (rentalDays <= 0) return new BadRequestObjectResult("Ngày thuê không hợp lệ");

            const decimal baseRentalFee = 20000;
            const int baseRentalDays = 60;
            const decimal extraFeePerDay = 1000;
            const decimal shippingFee = 20000;
            // Cong 3k khi qua ngay thue

            decimal totalFee = 0;
            decimal totalDeposit = 0;
            var orderId = Guid.NewGuid().ToString();

            var orderDetails = new List<RentOrderDetail>();

            foreach (var item in cart)
            {
                var rentBookItem = await _context.RentBookItems
                    .Include(r => r.RentBook)
                    .FirstOrDefaultAsync(x => x.RentBookItemId == item.RentBookItemId);

                if (rentBookItem == null || rentBookItem.RentBook == null)
                    continue;

                if (rentBookItem.Status != RentBookItemStatus.Available)
                    continue;

                decimal rentalFee = baseRentalFee;
                if (rentalDays > baseRentalDays)
                    rentalFee += (rentalDays - baseRentalDays) * extraFeePerDay;

                totalFee += rentalFee;
                totalDeposit += rentBookItem.RentBook.Price;

                orderDetails.Add(new RentOrderDetail
                {
                    OrderId = orderId,
                    RentBookItemId = rentBookItem.RentBookItemId,
                    BookTitle = rentBookItem.RentBook.Title,
                    BookPrice = rentBookItem.RentBook.Price,
                    RentalFee = rentalFee,
                    TotalFee = rentalFee,
                    Condition = rentBookItem.Condition
                });
            }

            if (!orderDetails.Any())
                return new BadRequestObjectResult("Không có sách hợp lệ trong giỏ hàng");

            if (request.HasShippingFee)
            {
                if (string.IsNullOrWhiteSpace(request.Address) || string.IsNullOrWhiteSpace(request.Phone))
                    return new BadRequestObjectResult("Vui lòng nhập địa chỉ và số điện thoại");

                totalFee += shippingFee;
            }

            var order = new RentOrder
            {
                OrderId = orderId,
                UserId = request.UserId,
                UserName = request.UserName,
                Address = request.Address,
                Phone = request.Phone,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RentalDays = rentalDays,
                HasShippingFee = request.HasShippingFee,
                ShippingFee = request.HasShippingFee ? shippingFee : 0,
                TotalDeposit = totalDeposit,
                TotalFee = totalFee + totalDeposit,
                OrderDate = DateTime.UtcNow,
                Status =OrderStatus.Pending ,
            };

            return new RentOrderResult
            {
                Order = order,
                Details = orderDetails
            };
        }

        private async Task SaveOrderAsync(RentOrderResult data)
        {
            await _context.RentOrders.AddAsync(data.Order);
            await _context.RentOrderDetails.AddRangeAsync(data.Details);

            // Đánh dấu RentBookItem đã được thuê
            foreach (var detail in data.Details)
            {
                var item = await _context.RentBookItems.FindAsync(detail.RentBookItemId);
                if (item != null)
                    item.Status = RentBookItemStatus.Rented;
            }

            await _context.SaveChangesAsync();
            _httpContextAccessor.HttpContext?.Session.Remove("RentalCart");
        }

    }
}
