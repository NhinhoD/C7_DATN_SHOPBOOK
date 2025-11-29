namespace ShopThueBanSach.Server.Models.AuthModel
{
	public class UpdateUserDto
	{

		public string? Address { get; set; }
		public string? Role { get; set; }
		public string? PhoneNumber { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public double? Points { get; set; }
		public IFormFile? ImageFile { get; set; }

	}
}