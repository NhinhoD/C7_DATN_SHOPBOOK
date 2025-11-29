using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShopThueBanSach.Server.Entities
{
    public class RentOrderDetail
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public RentOrder Order { get; set; }
        public string RentBookItemId { get; set; }
        public RentBookItem RentBookItem { get; set; }
        public string BookTitle { get; set; }
        public int Condition { get; set; }
		public string? ConditionDescription { get; set; }
		public decimal BookPrice { get; set; }
        public decimal RentalFee { get; set; }
        public decimal TotalFee { get; set; }
        public int? ReturnCondition { get; set; }
        public DateTime? ActualReturnDate { get; set; } // Added missing property  
        public decimal? ActualRefundAmount { get; set; }
    }
}
