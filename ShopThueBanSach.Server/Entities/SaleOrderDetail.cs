using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShopThueBanSach.Server.Entities
{
    public class SaleOrderDetail
    {
        [Key]
        public int Id { get; set; }

        public string OrderId { get; set; }
        [ForeignKey("OrderId")]
        public SaleOrder Order { get; set; }

        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
    }
}
