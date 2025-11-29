public class MonthlyRentBookStatisticsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<RentDayDataDto> DailyData { get; set; } = new();
}

public class RentDayDataDto
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalValue { get; set; }
}
