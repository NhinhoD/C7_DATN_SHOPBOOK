using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShopThueBanSach.Server.Entities
{
    public class Payment
    {
        [Key]
        public string PaymentId { get; set; } = Guid.NewGuid().ToString();

        public string OrderId { get; set; }
        [ForeignKey("OrderId")]
        public RentOrder Order { get; set; }

        public string Method { get; set; } // "cash" | "bank"
        public string Status { get; set; } = "Pending"; // "Pending", "Paid", "Failed"

        public DateTime? PaidAt { get; set; }
    }
}
