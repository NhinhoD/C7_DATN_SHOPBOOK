using System.ComponentModel.DataAnnotations;

namespace ShopThueBanSach.Server.Area.Admin.Model.StaffModel
{
	public class UpdateStaffDto
	{

		[Required]
		public string FullName { get; set; } = null!;


	

		[Required]
		public string PhoneNumber { get; set; } = null!;

		[Required]
		public string Address { get; set; } = null!;

		public DateTime? DateOfBirth { get; set; }

		public IFormFile? ImageFile { get; set; } // Hình ảnh đại diện
	}
}