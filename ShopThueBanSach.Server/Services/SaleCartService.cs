using Newtonsoft.Json;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Models.SaleModel.CartSaleModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
    public class SaleCartService : ISaleCartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDBContext _context;
        private const string CartKey = "SaleCart";

        public SaleCartService(IHttpContextAccessor accessor, AppDBContext context)
        {
            _httpContextAccessor = accessor;
            _context = context;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<CartItemSale> GetCart()
        {
            var json = Session.GetString(CartKey);
            return string.IsNullOrEmpty(json)
                ? new List<CartItemSale>()
                : JsonConvert.DeserializeObject<List<CartItemSale>>(json)!;
        }

        public void SaveCart(List<CartItemSale> cart)
        {
            var json = JsonConvert.SerializeObject(cart);
            Session.SetString(CartKey, json);
        }

        public void AddToCart(string productId, int quantity = 1)
        {
            if (quantity <= 0) return;

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);

            var product = _context.SaleBooks.FirstOrDefault(p => p.SaleBookId == productId);
            if (product == null || product.Quantity < 1) return;

            int cartQuantity = item?.Quantity ?? 0;
            int desiredQuantity = cartQuantity + quantity;

            if (desiredQuantity > product.Quantity) return;

            if (item != null)
            {
                item.Quantity = desiredQuantity;
            }
            else
            {
                cart.Add(new CartItemSale
                {
                    ProductId = product.SaleBookId,
                    ProductName = product.Title,
					ImageUrl = product.ImageUrl, // Hình ảnh sản phẩm, có thể null nếu không có
					UnitPrice = product.FinalPrice,
                    Quantity = quantity
                });
            }

            SaveCart(cart);
        }

        public void IncreaseQuantity(string productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return;

            var product = _context.SaleBooks.FirstOrDefault(p => p.SaleBookId == productId);
            if (product == null) return;

            if (item.Quantity + 1 <= product.Quantity)
            {
                item.Quantity++;
                SaveCart(cart);
            }
        }

        public void DecreaseQuantity(string productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return;

            item.Quantity--;

            if (item.Quantity <= 0)
                cart.Remove(item);

            SaveCart(cart);
        }

        public void UpdateQuantity(string productId, int quantity)
        {
            if (quantity <= 0) return;

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return;

            var product = _context.SaleBooks.FirstOrDefault(p => p.SaleBookId == productId);
            if (product == null || quantity > product.Quantity) return;

            item.Quantity = quantity;
            SaveCart(cart);
        }

        public void RemoveFromCart(string productId)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.ProductId == productId);
            SaveCart(cart);
        }

        public void ClearCart() => Session.Remove(CartKey);

        public decimal GetTotal() => GetCart().Sum(x => x.UnitPrice * x.Quantity);

        public List<CartItemSale> GetSelectedItems()
        {
            return GetCart().Where(x => x.IsSelected).ToList();
        }

        public void ToggleSelect(string productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                item.IsSelected = !item.IsSelected;
                SaveCart(cart);
            }
        }


    }
}
