using ShopThueBanSach.Server.Area.Admin.Model;
using ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Daily;
using ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Monthly;
using ShopThueBanSach.Server.Area.Admin.Model.ReportModel.Yearly;
using ShopThueBanSach.Server.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShopThueBanSach.Server.Area.Admin.Service.Interface
{
    public interface IReportService
    {
        // Tổng quan
        Task<OverviewStatisticsDto> GetOverviewStatisticsAsync();
        Task<OrderSummaryDto> GetOrderSummaryAsync();

        // Thống kê theo ngày (hôm nay)
        Task<DailySaleBookStatisticsDto> GetDailySaleBookStatisticsAsync();
        Task<DailyRentBookStatisticsDto> GetDailyRentBookStatisticsAsync();

        // ✅ Thống kê theo ngày chỉ định (POST ngày)


        // Thống kê theo tháng
        Task<MonthlySaleBookStatisticsDto> GetMonthlySaleBookStatisticsAsync();
        Task<MonthlyRentBookStatisticsDto> GetMonthlyRentBookStatisticsAsync();

        // Thống kê theo năm
        Task<YearlySaleBookStatisticsDto> GetYearlySaleBookStatisticsAsync();
        Task<YearlyRentBookStatisticsDto> GetYearlyRentBookStatisticsAsync();

        // Đơn hàng bán
        Task<List<SaleOrder>> GetSaleOrdersByDateAsync(DateTime date);
        Task<List<SaleOrder>> GetSaleOrdersByMonthAsync(int year, int month);
        Task<List<SaleOrder>> GetSaleOrdersByYearAsync(int year);

        // Đơn hàng thuê
        Task<List<RentOrder>> GetRentOrdersByDateAsync(DateTime date);
        Task<List<RentOrder>> GetRentOrdersByMonthAsync(int year, int month);
        Task<List<RentOrder>> GetRentOrdersByYearAsync(int year);
        Task SetDailySaleDateAsync(DateTime date);


        Task SetDailyRentDateAsync(DateTime date);
        Task SetMonthlySaleDateAsync(int year, int month);
        Task SetYearlySaleDateAsync(int year);

        Task SetMonthlyRentDateAsync(int year, int month);
        Task SetYearlyRentDateAsync(int year);
        //Xuất file excel
        Task<byte[]> ExportSaleReportToExcelAsync(DateTime? fromDate, DateTime? toDate);
        Task<byte[]> ExportRentReportToExcelAsync(DateTime? fromDate, DateTime? toDate);



    }
}
