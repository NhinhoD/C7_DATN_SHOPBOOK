namespace ShopThueBanSach.Server.Models.UserModel
{
    public class UpdateProfileDto
    {
        public string UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public IFormFile? ImageUser { get; set; }
    }
}
