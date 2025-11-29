using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;
/*using iText.Kernel.Pdf;*/
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShopThueBanSach.Server.Area.Admin.Model;
using ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Daily;
using ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Monthly;
using ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Yearly;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace ShopThueBanSach.Server.Area.Admin.Service
{
    public class ReportService : IReportService
    {
        private readonly AppDBContext _context;
        private readonly IMemoryCache _cache;

        public ReportService(AppDBContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }
        public async Task<byte[]> ExportSaleReportToExcelAsync(DateTime? fromDate, DateTime? toDate)
        {
            // Kiểm tra ngày nhập vào
            if (fromDate.HasValue && toDate.HasValue)
            {
                if (fromDate.Value > toDate.Value)
                    throw new ArgumentException("Ngày bắt đầu không thể lớn hơn ngày kết thúc.");

                if (fromDate.Value == DateTime.Today || toDate.Value == DateTime.Today)
                    throw new ArgumentException("Ngày không hợp lệ.");
            }

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Báo cáo bán sách");

            // 1. Thời gian xuất file - Hàng 1
            worksheet.Cell(1, 1).Value = $"Thời gian xuất file: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}";
            worksheet.Range(1, 1, 1, 8).Merge();
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 2. Khoảng thời gian lọc - Hàng 2
            var fromStr = fromDate.HasValue ? fromDate.Value.ToString("dd/MM/yyyy") : "Không xác định";
            var toStr = toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "Không xác định";
            worksheet.Cell(2, 1).Value = $"Khoảng thời gian lọc: {fromStr} - {toStr}";
            worksheet.Range(2, 1, 2, 8).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 3. Header - Hàng 3
            worksheet.Cell(3, 1).Value = "STT";
            worksheet.Cell(3, 2).Value = "Mã đơn hàng";
            worksheet.Cell(3, 3).Value = "Ngày bán";
            worksheet.Cell(3, 4).Value = "Khách hàng";
            worksheet.Cell(3, 5).Value = "Số lượng sách";
            worksheet.Cell(3, 6).Value = "Tổng tiền";
            worksheet.Cell(3, 7).Value = "Phí vận chuyển";
            worksheet.Cell(3, 8).Value = "Giảm giá";

            // 4. Truy vấn dữ liệu
            var query = _context.SaleOrders
                .Include(o => o.SaleOrderDetails)
                .Where(o => o.Status == OrderStatus.Completed);

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);

            var orders = await query.ToListAsync();

            // 5. Ghi dữ liệu - bắt đầu từ hàng 4
            for (int i = 0; i < orders.Count; i++)
            {
                var row = i + 4;
                var order = orders[i];

                worksheet.Cell(row, 1).Value = i + 1;
                worksheet.Cell(row, 2).Value = order.OrderId;
                worksheet.Cell(row, 3).Value = order.OrderDate.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 4).Value = order.UserId; // Nếu cần hiển thị tên, thay bằng order.User?.UserName
                worksheet.Cell(row, 5).Value = order.SaleOrderDetails.Sum(d => d.Quantity);
                worksheet.Cell(row, 6).Value = order.TotalAmount;
                worksheet.Cell(row, 7).Value = order.HasShippingFee ? order.ShippingFee : 0;
                worksheet.Cell(row, 8).Value = order.DiscountAmount;
            }

            // 6. Format tiền tệ
            worksheet.Column(6).Style.NumberFormat.Format = "#,##0";
            worksheet.Column(7).Style.NumberFormat.Format = "#,##0";
            worksheet.Column(8).Style.NumberFormat.Format = "#,##0";

            // 7. Tự động điều chỉnh độ rộng cột
            worksheet.Columns().AdjustToContents();

            // 8. Ghi ra stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportRentReportToExcelAsync(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue && toDate.HasValue)
            {
                if (fromDate.Value > toDate.Value)
                    throw new ArgumentException("Ngày bắt đầu không thể lớn hơn ngày kết thúc.");

                if (fromDate.Value == DateTime.Today || toDate.Value == DateTime.Today)
                    throw new ArgumentException("Ngày không hợp lệ.");
            }

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Báo cáo thuê sách");

            // 1. Thời gian xuất file - Hàng 1
            worksheet.Cell(1, 1).Value = $"Thời gian xuất file: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}";
            worksheet.Range(1, 1, 1, 8).Merge();
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 2. Khoảng thời gian lọc - Hàng 2
            var fromDateStr = fromDate.HasValue ? fromDate.Value.ToString("dd/MM/yyyy") : "Không xác định";
            var toDateStr = toDate.HasValue ? toDate.Value.ToString("dd/MM/yyyy") : "Không xác định";
            worksheet.Cell(2, 1).Value = $"Khoảng thời gian lọc: {fromDateStr} - {toDateStr}";
            worksheet.Range(2, 1, 2, 8).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // 3. Header - Hàng 3
            worksheet.Cell(3, 1).Value = "STT";
            worksheet.Cell(3, 2).Value = "Mã đơn hàng";
            worksheet.Cell(3, 3).Value = "Ngày thuê";
            worksheet.Cell(3, 4).Value = "Ngày trả thực tế";
            worksheet.Cell(3, 5).Value = "Khách hàng";
            worksheet.Cell(3, 6).Value = "Số lượng sách";
            worksheet.Cell(3, 7).Value = "Tổng tiền";
            worksheet.Cell(3, 8).Value = "Phí vận chuyển";
            

            // 4. Truy vấn dữ liệu
            var query = _context.RentOrders
                .Include(o => o.RentOrderDetails)
                .Where(o => o.Status == OrderStatus.Completed);

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);

            var orders = await query.ToListAsync();

            // 5. Ghi dữ liệu - bắt đầu từ hàng 4
            for (int i = 0; i < orders.Count; i++)
            {
                var row = i + 4;
                var order = orders[i];

                worksheet.Cell(row, 1).Value = i + 1;
                worksheet.Cell(row, 2).Value = order.OrderId;
                worksheet.Cell(row, 3).Value = order.OrderDate.ToString("dd/MM/yyyy");
                 worksheet.Cell(row, 4).Value = order.ActualReturnDate.HasValue
                    ? order.ActualReturnDate.Value.ToString("dd/MM/yyyy")
                    : "Chưa trả";
                worksheet.Cell(row, 5).Value = order.User?.UserName ?? order.UserId;
                worksheet.Cell(row, 6).Value = order.RentOrderDetails.Count;
                worksheet.Cell(row, 7).Value = order.TotalFee;
                worksheet.Cell(row, 8).Value = order.HasShippingFee ? order.ShippingFee : 0;
               
            }

            // 6. Định dạng cột tiền
            worksheet.Column(6).Style.NumberFormat.Format = "#,##0";
            worksheet.Column(7).Style.NumberFormat.Format = "#,##0";

            // 7. Tự động điều chỉnh độ rộng cột
            worksheet.Columns().AdjustToContents();

            // 8. Ghi ra memory stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }


        /*  public async Task<byte[]> ExportSaleReportToPdfAsync(DateTime? fromDate, DateTime? toDate)
		  {
			  // Kiểm tra ngày nhập vào
			  if (fromDate.HasValue && toDate.HasValue)
			  {
				  if (fromDate.Value > toDate.Value)
				  {
					  throw new ArgumentException("Ngày bắt đầu không thể lớn hơn ngày kết thúc.");
				  }

				  if (fromDate.Value > DateTime.Today || toDate.Value > DateTime.Today)
				  {
					  throw new ArgumentException("Ngày không được nằm trong tương lai.");
				  }
			  }

			  using var memoryStream = new MemoryStream();
			  using (var writer = new PdfWriter(memoryStream))
			  using (var pdf = new PdfDocument(writer))
			  {
				  var document = new Document(pdf);
				  document.Add(new Paragraph("Báo cáo bán sách")
					  .SetTextAlignment(TextAlignment.CENTER)
					  .SetFontSize(20));
  // Ghi thời gian xuất file
				  document.Add(new Paragraph($"Thời gian xuất file: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}")
					  .SetTextAlignment(TextAlignment.CENTER));

				  // Header
				  var table = new Table(8);
				  table.AddHeaderCell("STT");
				  table.AddHeaderCell("Mã đơn hàng");
				  table.AddHeaderCell("Ngày đặt");
				  table.AddHeaderCell("Khách hàng");
				  table.AddHeaderCell("Số lượng sách");
				  table.AddHeaderCell("Tổng tiền");
				  table.AddHeaderCell("Phí vận chuyển");
				  table.AddHeaderCell("Giảm giá");

				  // Query data
				  var query = _context.SaleOrders
					  .Include(o => o.SaleOrderDetails)
					  .Where(o => o.Status != OrderStatus.Canceled);

				  if (fromDate.HasValue)
					  query = query.Where(o => o.OrderDate >= fromDate.Value);

				  if (toDate.HasValue)
					  query = query.Where(o => o.OrderDate <= toDate.Value);

				  var orders = await query.ToListAsync();

				  // Fill data
				  for (int i = 0; i < orders.Count; i++)
				  {
					  var order = orders[i];
					  table.AddCell((i + 1).ToString());
					  table.AddCell(order.OrderId);
					  table.AddCell(order.OrderDate.ToString("dd/MM/yyyy"));
					  table.AddCell(order.UserId); // Giả sử UserId là tên khách hàng
					  table.AddCell(order.SaleOrderDetails.Sum(d => d.Quantity).ToString());
					  table.AddCell(order.TotalAmount.ToString("N0"));
					  table.AddCell(order.HasShippingFee ? order.ShippingFee.ToString("N0") : "0");
					  table.AddCell(order.DiscountAmount.ToString("N0"));
				  }

				  document.Add(table);
				  document.Close();
			  }

			  return memoryStream.ToArray();
		  }
		  public async Task<byte[]> ExportRentReportToPdfAsync(DateTime? fromDate, DateTime? toDate)
  {
	  // Kiểm tra ngày nhập vào
	  if (fromDate.HasValue && toDate.HasValue)
	  {
		  if (fromDate.Value > toDate.Value)
		  {
			  throw new ArgumentException("Ngày bắt đầu không thể lớn hơn ngày kết thúc.");
		  }

		  if (fromDate.Value > DateTime.Today || toDate.Value > DateTime.Today)
		  {
			  throw new ArgumentException("Ngày không được nằm trong tương lai.");
		  }
	  }

	  using var memoryStream = new MemoryStream();
	  using (var writer = new PdfWriter(memoryStream))
	  using (var pdf = new PdfDocument(writer))
	  {
		  var document = new Document(pdf);
		  document.Add(new Paragraph("Báo cáo thuê sách")
			  .SetTextAlignment(TextAlignment.CENTER)
			  .SetFontSize(20));

		  // Ghi thời gian xuất file
  document.Add(new Paragraph($"Thời gian xuất file: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}")
			  .SetTextAlignment(TextAlignment.CENTER));

		  // Header
		  var table = new Table(7);
		  table.AddHeaderCell("STT");
		  table.AddHeaderCell("Mã đơn hàng");
		  table.AddHeaderCell("Ngày đặt");
		  table.AddHeaderCell("Khách hàng");
		  table.AddHeaderCell("Số lượng sách");
		  table.AddHeaderCell("Tổng tiền");
		  table.AddHeaderCell("Phí vận chuyển");

		  // Query data
		  var query = _context.RentOrders
			  .Include(o => o.RentOrderDetails)
			  .Where(o => o.Status != OrderStatus.Canceled);

		  if (fromDate.HasValue)
			  query = query.Where(o => o.OrderDate >= fromDate.Value);

		  if (toDate.HasValue)
			  query = query.Where(o => o.OrderDate <= toDate.Value);

		  var orders = await query.ToListAsync();

		  // Fill data
		  for (int i = 0; i < orders.Count; i++)
		  {
			  var order = orders[i];
			  table.AddCell((i + 1).ToString());
			  table.AddCell(order.OrderId);
			  table.AddCell(order.OrderDate.ToString("dd/MM/yyyy"));
			  table.AddCell(order.UserId); // Giả sử UserId là tên khách hàng
			  table.AddCell(order.RentOrderDetails.Count.ToString());
			  table.AddCell(order.TotalFee.ToString("N0"));
			  table.AddCell(order.HasShippingFee ? order.ShippingFee.ToString("N0") : "0");
		  }

		  document.Add(table);
		  document.Close();
	  }

	  return memoryStream.ToArray();
  }*/

        public Task SetDailySaleDateAsync(DateTime date)
        {
            if (date.Date > DateTime.Today)
                throw new ArgumentException("Ngày không hợp lệ vui lòng nhập lại");

            _cache.Set("DailySaleDate", date.Date, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        public Task SetDailyRentDateAsync(DateTime date)
        {
            if (date.Date > DateTime.Today)
                throw new ArgumentException("Ngày không hợp lệ vui lòng nhập lại");

            _cache.Set("DailyRentDate", date.Date, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        public Task SetMonthlySaleDateAsync(int year, int month)
        {
            var now = DateTime.Today;
            if (year > now.Year || (year == now.Year && month > now.Month))
                throw new ArgumentException("Tháng hoặc năm không hợp lệ");

            _cache.Set("MonthlySaleDate", (year, month), TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        public Task SetYearlySaleDateAsync(int year)
        {
            if (year > DateTime.Today.Year)
                throw new ArgumentException("Năm không hợp lệ");

            _cache.Set("YearlySaleDate", year, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }
        public Task SetMonthlyRentDateAsync(int year, int month)
        {
            var now = DateTime.Today;
            if (year > now.Year || (year == now.Year && month > now.Month))
                throw new ArgumentException("Tháng hoặc năm không hợp lệ");

            _cache.Set("MonthlyRentDate", (year, month), TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        public Task SetYearlyRentDateAsync(int year)
        {
            if (year > DateTime.Today.Year)
                throw new ArgumentException("Năm không hợp lệ");

            _cache.Set("YearlyRentDate", year, TimeSpan.FromDays(1));
            return Task.CompletedTask;
        }

        public async Task<OverviewStatisticsDto> GetOverviewStatisticsAsync()
        {
            var saleBooks = await _context.SaleBooks.AsNoTracking().ToListAsync();

            return new OverviewStatisticsDto
            {
                TotalSaleBooks = saleBooks.Sum(x => x.Quantity),
                TotalSaleBookValue = saleBooks.Sum(x => x.Price * x.Quantity)
            };
        }

        public async Task<OrderSummaryDto> GetOrderSummaryAsync()
        {
            var saleOrders = _context.SaleOrders.AsNoTracking().Where(o => o.Status != OrderStatus.Canceled);
            var rentOrders = _context.RentOrders.AsNoTracking().Where(o => o.Status != OrderStatus.Canceled);

            return new OrderSummaryDto
            {
                TotalSaleOrders = await saleOrders.CountAsync(),
                TotalSaleAmount = await saleOrders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                TotalRentOrders = await rentOrders.CountAsync(),
                TotalRentAmount = await rentOrders.SumAsync(o => (decimal?)o.TotalFee) ?? 0
            };
        }

        public async Task<DailySaleBookStatisticsDto> GetDailySaleBookStatisticsAsync()
        {
            var selectedDate = _cache.TryGetValue("DailySaleDate", out DateTime cachedDate)
                ? cachedDate
                : DateTime.Today;

            var saleOrders = await _context.SaleOrders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate.Date == selectedDate)
                .ToListAsync();

            var ordersDetail = saleOrders.Select(o => new SaleOrderInDayDto
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                CreatedDate = o.OrderDate,
                TotalValue = o.TotalAmount
            }).ToList();

            return new DailySaleBookStatisticsDto
            {
                CreatedDate = selectedDate,
                OrdersToday = saleOrders.Count,
                TotalValueToday = saleOrders.Sum(o => o.TotalAmount),
                Statuses = saleOrders.Select(o => o.Status).Distinct().ToList(),
                Orders = ordersDetail
            };
        }


        public async Task<MonthlySaleBookStatisticsDto> GetMonthlySaleBookStatisticsAsync()
        {
            var (year, month) = _cache.TryGetValue("MonthlySaleDate", out ValueTuple<int, int> cached)
                ? cached
                : (DateTime.Today.Year, DateTime.Today.Month);

            var saleOrders = await GetSaleOrdersByMonthAsync(year, month);

            // 👉 Lọc chỉ các đơn đã hoàn thành
            var completedOrders = saleOrders
                .Where(o => o.Status == OrderStatus.Completed)
                .ToList();

            var dailyGroups = completedOrders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new SaleDayDataDto
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    TotalValue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(d => d.Date)
                .ToList();

            return new MonthlySaleBookStatisticsDto
            {
                Year = year,
                Month = month,
                DailyData = dailyGroups
            };
        }




        public async Task<YearlySaleBookStatisticsDto> GetYearlySaleBookStatisticsAsync()
        {
            int year = _cache.TryGetValue("YearlySaleDate", out int cachedYear)
                ? cachedYear
                : DateTime.Today.Year;

            var saleOrders = await _context.SaleOrders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed && o.OrderDate.Year == year)
                .ToListAsync();

            var monthlyData = new List<YearlySaleMonthDataDto>();

            for (int month = 1; month <= 12; month++)
            {
                var monthOrders = saleOrders
                    .Where(o => o.OrderDate.Month == month)
                    .ToList();

                monthlyData.Add(new YearlySaleMonthDataDto
                {
                    Month = month,
                    Orders = monthOrders.Count,
                    TotalValue = monthOrders.Sum(o => o.TotalAmount),
                    CreatedDates = monthOrders
                        .Select(o => o.OrderDate.Date)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList()
                });
            }

            return new YearlySaleBookStatisticsDto
            {
                Year = year,
                MonthlyData = monthlyData
            };
        }

        public async Task<List<SaleOrder>> GetSaleOrdersByDateAsync(DateTime date)
        {
            return await _context.SaleOrders
                .Include(o => o.SaleOrderDetails)
                .Where(o => o.Status != OrderStatus.Canceled && o.OrderDate.Date == date.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SaleOrder>> GetSaleOrdersByMonthAsync(int year, int month)
        {
            var now = DateTime.Today;
            if (year > now.Year || (year == now.Year && month > now.Month))
                throw new ArgumentException("Tháng hoặc năm không hợp lẹ. Vui lòng nhập lại");

            return await _context.SaleOrders
                .Include(o => o.SaleOrderDetails)
                .Where(o => o.Status != OrderStatus.Canceled &&
                            o.OrderDate.Year == year &&
                            o.OrderDate.Month == month)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<SaleOrder>> GetSaleOrdersByYearAsync(int year)
        {
            if (year > DateTime.Today.Year)
                throw new ArgumentException("Năm không hợp lệ vui lòng nhập lại");

            return await _context.SaleOrders
                .Include(o => o.SaleOrderDetails)
                .Where(o => o.Status != OrderStatus.Canceled &&
                            o.OrderDate.Year == year)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<DailyRentBookStatisticsDto> GetDailyRentBookStatisticsAsync()
        {
            var selectedDate = _cache.TryGetValue("DailyRentDate", out DateTime cachedDate)
                ? cachedDate
                : DateTime.Today;

            var rentOrders = await _context.RentOrders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed &&
                            o.ActualReturnDate.HasValue &&
                            o.ActualReturnDate.Value.Date == selectedDate)
                .ToListAsync();

            var ordersDetail = rentOrders.Select(o => new RentOrderInDayDto
            {
                OrderId = o.OrderId,
                UserId = o.UserId,
                ActualReturnDate = o.ActualReturnDate!.Value,
                TotalValue = o.TotalFee - (o.ActualRefundAmount ?? 0)
            }).ToList();

            return new DailyRentBookStatisticsDto
            {
                ActualReturnDate = selectedDate,
                OrdersToday = rentOrders.Count,
                TotalValueToday = rentOrders.Sum(o => o.TotalFee - (o.ActualRefundAmount ?? 0)),
                Statuses = rentOrders.Select(o => o.Status).Distinct().ToList(),
                Orders = ordersDetail
            };
        }


        public async Task<MonthlyRentBookStatisticsDto> GetMonthlyRentBookStatisticsAsync()
        {
            // Lấy năm và tháng từ cache
            var (year, month) = _cache.TryGetValue("MonthlyRentDate", out (int year, int month) cached)
                ? cached
                : (DateTime.Today.Year, DateTime.Today.Month);

            var rentOrders = await _context.RentOrders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed &&
                            o.ActualReturnDate.HasValue &&
                            o.ActualReturnDate.Value.Year == year &&
                            o.ActualReturnDate.Value.Month == month)
                .ToListAsync();

            var dailyData = rentOrders
                .GroupBy(o => o.ActualReturnDate!.Value.Date)
                .Select(g => new RentDayDataDto
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    TotalValue = g.Sum(o => o.TotalFee - (o.ActualRefundAmount ?? 0))
                })
                .OrderBy(d => d.Date)
                .ToList();

            return new MonthlyRentBookStatisticsDto
            {
                Year = year,
                Month = month,
                DailyData = dailyData
            };
        }



        public async Task<YearlyRentBookStatisticsDto> GetYearlyRentBookStatisticsAsync()
        {
            if (!_cache.TryGetValue("YearlyRentDate", out int year))
            {
                year = DateTime.Today.Year;
            }

            var rentOrders = await _context.RentOrders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed &&
                            o.ActualReturnDate.HasValue &&
                            o.ActualReturnDate.Value.Year == year)
                .ToListAsync();

            var monthlyData = new List<YearlyRentMonthDataDto>();

            for (int month = 1; month <= 12; month++)
            {
                var monthOrders = rentOrders
                    .Where(o => o.ActualReturnDate!.Value.Month == month)
                    .ToList();

                var returnDates = monthOrders
                    .Select(o => o.ActualReturnDate!.Value.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                monthlyData.Add(new YearlyRentMonthDataDto
                {
                    Month = month,
                    Orders = monthOrders.Count,
                    TotalValue = monthOrders.Sum(o => o.TotalFee - (o.ActualRefundAmount ?? 0)),
                    ReturnDates = returnDates
                });
            }

            return new YearlyRentBookStatisticsDto
            {
                Year = year,
                MonthlyData = monthlyData
            };
        }



        public async Task<List<RentOrder>> GetRentOrdersByDateAsync(DateTime date)
        {
            return await _context.RentOrders
                .Include(o => o.RentOrderDetails)
                .ThenInclude(d => d.RentBookItem)
                .ThenInclude(item => item.RentBook)
.Where(o => o.Status != OrderStatus.Canceled && o.OrderDate.Date == date.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<RentOrder>> GetRentOrdersByMonthAsync(int year, int month)
        {
            var now = DateTime.Today;
            if (year > now.Year || (year == now.Year && month > now.Month))
                throw new ArgumentException("Tháng hoặc năm không hợp lệ. Vui lòng nhập lại");

            return await _context.RentOrders
                .Include(o => o.RentOrderDetails)
                .ThenInclude(d => d.RentBookItem)
                .ThenInclude(item => item.RentBook)
                .Where(o => o.Status != OrderStatus.Canceled &&
                            o.OrderDate.Year == year &&
                            o.OrderDate.Month == month)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<RentOrder>> GetRentOrdersByYearAsync(int year)
        {
            if (year > DateTime.Today.Year)
                throw new ArgumentException("Năm không hợp lệ vui lòng nhập lại.");

            return await _context.RentOrders
                .Include(o => o.RentOrderDetails)
                .ThenInclude(d => d.RentBookItem)
                .ThenInclude(item => item.RentBook)
                .Where(o => o.Status != OrderStatus.Canceled &&
                            o.OrderDate.Year == year)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}//