using System.ComponentModel.DataAnnotations;

namespace ShopThueBanSach.Server.Models.BooksModel.DiscountCode
{
    // Dùng cho TẠO mới
    public class CreateDiscountCodeDto
    {
        [Required(ErrorMessage = "Tên mã giảm giá là bắt buộc.")]
        public string DiscountCodeName { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int AvailableQuantity { get; set; }

        [Required(ErrorMessage = "Điểm yêu cầu là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Điểm yêu cầu không được âm.")]
        public int RequiredPoints { get; set; }

        [Required(ErrorMessage = "Giá trị giảm là bắt buộc.")]
        [Range(1, 100, ErrorMessage = "Giá trị giảm phải từ 1% đến 100%.")]
        public double DiscountValue { get; set; }
    }
}
