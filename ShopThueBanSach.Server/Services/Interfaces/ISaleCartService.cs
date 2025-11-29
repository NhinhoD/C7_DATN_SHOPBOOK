using ShopThueBanSach.Server.Models.SaleModel.CartSaleModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface ISaleCartService
    {
        List<CartItemSale> GetCart();
        List<CartItemSale> GetSelectedItems();
        void ToggleSelect(string productId);
        void AddToCart(string productId, int quantity = 1);
        void IncreaseQuantity(string productId);
        void DecreaseQuantity(string productId);
        void RemoveFromCart(string productId);
        void ClearCart();
        decimal GetTotal();
        void SaveCart(List<CartItemSale> cart);
    }
}
