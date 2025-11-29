namespace ShopThueBanSach.Server.Area.Admin.Model
{
    public class RentOrderDetailDto
    {
        public int Id { get; set; }

        public string BookTitle { get; set; } = string.Empty;
        public decimal BookPrice { get; set; }

        public int Condition { get; set; }
		public string? ConditionDescription { get; set; }
		public string? StatusDescription { get; set; }
		public int? ReturnCondition { get; set; }

        public decimal RentalFee { get; set; }

        public decimal TotalFee { get; set; }

        public decimal? ActualRefundAmount { get; set; }

        public DateTime? ActualReturnDate { get; set; }
    }
}
