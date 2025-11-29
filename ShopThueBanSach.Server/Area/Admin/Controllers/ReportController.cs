using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using System;
using System.Threading.Tasks;
using ShopThueBanSach.Server.Area.Admin.Model.Request;

namespace ShopThueBanSach.Server.Area.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "Admin")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // Tổng hợp đơn hàng Sale
        [HttpGet]
        public async Task<IActionResult> GetOrdersSummary()
        {
            var result = await _reportService.GetOrderSummaryAsync();
            return Ok(result);
        }

        // --------------------------- SALE ---------------------------

        // GET Sale - Daily
        [HttpGet("sale/daily")]
        public async Task<IActionResult> GetDailySaleStatistics()
        {
            var result = await _reportService.GetDailySaleBookStatisticsAsync();
            return Ok(result);
        }

        [HttpPost("sale/daily/set-date")]
        public async Task<IActionResult> SetDailySaleDate([FromBody] DateTime date)
        {
            if (date.Date > DateTime.Today)
                return BadRequest(new { error = "Ngày không hợp lệ vui lòng nhập lại" });

            await _reportService.SetDailySaleDateAsync(date);
            return Ok(new { message = $"Ngày thống kê bán được chọn là {date:dd/MM/yyyy}" });
        }

        // GET Sale - Monthly
        [HttpGet("sale/monthly")]
        public async Task<IActionResult> GetMonthlySaleStatistics()
        {
            var result = await _reportService.GetMonthlySaleBookStatisticsAsync();
            return Ok(result);
        }

        [HttpPost("sale/monthly/set-date")]
        public async Task<IActionResult> SetMonthlySaleDate([FromBody] SaleOrderByMonthRequest request)
        {
            try
            {
                await _reportService.SetMonthlySaleDateAsync(request.Year, request.Month);
                return Ok(new { message = $"Đã chọn tháng {request.Month}/{request.Year} để thống kê bán" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET Sale - Yearly
        [HttpGet("sale/yearly")]
        public async Task<IActionResult> GetYearlySaleStatistics()
        {
            var result = await _reportService.GetYearlySaleBookStatisticsAsync();
            return Ok(result);
        }

        [HttpPost("sale/yearly/set-date")]
        public async Task<IActionResult> SetYearlySaleDate([FromBody] SaleOrderByYearRequest request)
        {
            try
            {
                await _reportService.SetYearlySaleDateAsync(request.Year);
                return Ok(new { message = $"Đã chọn năm {request.Year} để thống kê bán" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // --------------------------- RENT ---------------------------

        // GET Rent - Daily
        [HttpGet("rent/daily")]
        public async Task<IActionResult> GetDailyRentStatistics()
        {
            var result = await _reportService.GetDailyRentBookStatisticsAsync();
            return Ok(result);
        }

        [HttpPost("rent/daily/set-date")]
        public async Task<IActionResult> SetDailyRentDate([FromBody] DateTime date)
        {
            if (date.Date > DateTime.Today)
                return BadRequest(new { error = "Ngày không hợp lệ vui lòng nhập lại" });

            await _reportService.SetDailyRentDateAsync(date);
            return Ok(new { message = $"Ngày thống kê thuê được chọn là {date:dd/MM/yyyy}" });
        }

        // GET Rent - Monthly
        [HttpGet("rent/monthly")]
        public async Task<IActionResult> GetMonthlyRentStatistics()
        {
            var result = await _reportService.GetMonthlyRentBookStatisticsAsync();
            return Ok(result);
        }

        [HttpPost("rent/monthly/set-date")]
        public async Task<IActionResult> SetMonthlyRentDate([FromBody] RentOrderByMonthRequest request)
        {
            var now = DateTime.Today;
            if (request.Year > now.Year || (request.Year == now.Year && request.Month > now.Month))
                return BadRequest(new { error = "Tháng hoặc năm không hợp lệ. Vui lòng nhập lại" });

            await _reportService.SetMonthlyRentDateAsync(request.Year, request.Month);
            return Ok(new { message = $"Đã chọn tháng {request.Month}/{request.Year} để thống kê thuê" });
        }
        // GET Rent - Yearly
        [HttpGet("rent/yearly")]
        public async Task<IActionResult> GetYearlyRentStatistics()
        {
            var result = await _reportService.GetYearlyRentBookStatisticsAsync();
            return Ok(result);
        }

        // GET Rent - Yearly
        [HttpPost("rent/yearly/set-date")]
        public async Task<IActionResult> SetYearlyRentDate([FromBody] RentOrderByYearRequest request)
        {
            if (request.Year > DateTime.Today.Year)
                return BadRequest(new { error = "Năm không hợp lệ. Vui lòng nhập lại" });

            await _reportService.SetYearlyRentDateAsync(request.Year);
            return Ok(new { message = $"Đã chọn năm {request.Year} để thống kê thuê" });
        }
        // POST Export Sale Report
        [HttpPost("sale/export")]
        public async Task<IActionResult> ExportSaleReport([FromBody] ExportReportRequest request)
        {
            try
            {
                var excelData = await _reportService.ExportSaleReportToExcelAsync(request.FromDate, request.ToDate);
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"BaoCaoBanSach_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi khi xuất báo cáo: {ex.Message}" });
            }
        }
        // POST Export Rent Report
        [HttpPost("rent/export")]
        public async Task<IActionResult> ExportRentReport([FromBody] ExportReportRequest request)
        {
            try
            {
                var excelData = await _reportService.ExportRentReportToExcelAsync(request.FromDate, request.ToDate);
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"BaoCaoThueSach_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Lỗi khi xuất báo cáo: {ex.Message}" });
            }
        }

        /*  [HttpPost("rent-orders-by-year")]
          public async Task<IActionResult> GetRentOrdersByYear([FromBody] RentOrderByYearRequest request)
          {
              if (request.Year > DateTime.Today.Year)
                  return BadRequest(new { error = "Năm không hợp lệ vui lòng nhập lại" });

              var result = await _reportService.GetRentOrdersByYearAsync(request.Year);
              return Ok(result);
          }*/
    }
}