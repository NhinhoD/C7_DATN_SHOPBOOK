namespace ShopThueBanSach.Server.Models.UserModel
{
    public class UserProfileDto
    {
		public string UserId { get; set; } = string.Empty;
		public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhonNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public double Points { get; set; }
        public string? ImageUser { get; set; }
    }
}
