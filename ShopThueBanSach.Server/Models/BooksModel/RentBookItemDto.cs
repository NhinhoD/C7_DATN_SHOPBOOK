using ShopThueBanSach.Server.Entities;
using System.ComponentModel.DataAnnotations;

namespace ShopThueBanSach.Server.Models.BooksModel
{
    public class RentBookItemDto
    {
        public string? RentBookItemId { get; set; }

        [Required]
        public string RentBookId { get; set; } = null!;
        public string? RentBookTitle { get; set; }

        [Required]
        public RentBookItemStatus Status { get; set; }

        [Required]
        public string StatusDescription { get; set; }

		[Range(0, 100, ErrorMessage = "Condition must be between 0 and 100")]
        public int Condition { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsHidden { get; set; }
    }
}
