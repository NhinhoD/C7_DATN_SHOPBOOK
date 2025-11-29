using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ShopThueBanSach.Server.Models.StaffModel
{
    public class StaffDto
    {

        // StaffDto.cs

        public string? StaffId { get; set; } // Giữ lại để dùng nội bộ trong code, không bắt client truyền vào



        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        public IFormFile? ImageFile { get; set; } // Hình ảnh đại diện
    }
}
