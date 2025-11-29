namespace ShopThueBanSach.Server.Models.SaleModel.CartSaleModel
{
    public class AddToSaleCartDto
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
