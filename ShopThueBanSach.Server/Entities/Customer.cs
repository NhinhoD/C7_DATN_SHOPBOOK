namespace ShopThueBanSach.Server.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; } // "KH", "NV", "AD"
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public int LoyaltyPoints { get; set; }
        public bool IsBanned { get; set; } = false;
    }

}
