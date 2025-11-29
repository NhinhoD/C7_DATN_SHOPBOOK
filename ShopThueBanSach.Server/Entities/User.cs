using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace ShopThueBanSach.Server.Entities
{
	public class User : IdentityUser
	{
		//public string? Role { get; set; }
		public string? Address { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public double Points { get; set; }
		public string? ImageUser { get; set; }
		[JsonIgnore] // Bỏ qua khi serialize JSON
		public ICollection<FavoriteBook> FavoriteBooks { get; set; }

		[JsonIgnore]
		public ICollection<FavoriteRentBook> FavoriteRentBooks { get; set; }


		[JsonIgnore]
		public ICollection<Voucher> Vouchers { get; set; }
	}
}