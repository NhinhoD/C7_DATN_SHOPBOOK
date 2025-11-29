using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Models.CartRentModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
    public class CartRentService : ICartRentService
    {
        private readonly AppDBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SessionKey = "RentalCart";

        public CartRentService(IHttpContextAccessor httpContextAccessor, AppDBContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<CartItemRent> GetCart()
        {
            var json = Session.GetString(SessionKey);
            return string.IsNullOrEmpty(json)
                ? new List<CartItemRent>()
                : JsonConvert.DeserializeObject<List<CartItemRent>>(json)!;
        }

        private void SaveCart(List<CartItemRent> cart)
        {
            var json = JsonConvert.SerializeObject(cart);
            Session.SetString(SessionKey, json);
        }

        public async Task<bool> AddToCartAsync(string rentBookItemId)
        {
            var cart = GetCart();

            if (cart.Any(x => x.RentBookItemId == rentBookItemId))
                return false; // Đã có

            var item = await _context.RentBookItems
                .Include(x => x.RentBook)
                .FirstOrDefaultAsync(x => x.RentBookItemId == rentBookItemId && x.Status == Entities.RentBookItemStatus.Available);

            if (item == null || item.RentBook == null) return false;

            var rentalFee = CalculateRentalFee(60); // mặc định 60 ngày

            var cartItem = new CartItemRent
            {
                RentBookItemId = item.RentBookItemId,
                RentBookTitle = item.RentBook.Title,
                BookPrice = item.RentBook.Price,
				imageUrl = item.RentBook.ImageUrl, // Hình ảnh sách, có thể null nếu không có
				Condition = item.Condition,
                RentalFee = rentalFee,
                TotalFee = rentalFee,
                IsSelected = true
            };

            cart.Add(cartItem);
            SaveCart(cart);
            return true;
        }

        public void RemoveFromCart(string rentBookItemId)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.RentBookItemId == rentBookItemId);
            SaveCart(cart);
        }

        public void ToggleSelect(string rentBookItemId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.RentBookItemId == rentBookItemId);
            if (item != null)
            {
                item.IsSelected = !item.IsSelected;
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            Session.Remove(SessionKey);
        }

        public List<CartItemRent> GetSelectedItems()
        {
            return GetCart().Where(x => x.IsSelected).ToList();
        }

        public void RecalculateRentalFee(DateTime startDate, DateTime endDate)
        {
            var cart = GetCart();
            int rentalDays = (endDate - startDate).Days;
            if (rentalDays <= 0) rentalDays = 1;

            foreach (var item in cart)
            {
                item.RentalFee = CalculateRentalFee(rentalDays);
                item.TotalFee = item.RentalFee;
            }

            SaveCart(cart);
        }

        private decimal CalculateRentalFee(int rentalDays)
        {
            const decimal baseFee = 30000m;
            const int baseDays = 60;
            const decimal extraPerDay = 1000m;

            if (rentalDays <= baseDays)
                return baseFee;

            int extraDays = rentalDays - baseDays;
            return baseFee + (extraDays * extraPerDay);
        }
    }
}
