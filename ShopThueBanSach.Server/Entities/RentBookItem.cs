using System.ComponentModel.DataAnnotations;

namespace ShopThueBanSach.Server.Entities
{
    public enum RentBookItemStatus
    {
        Available = 0,
        Rented = 1
    }

    public class RentBookItem
    {
        [Key]
        public string RentBookItemId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string RentBookId { get; set; } = null!;

        [Required]
        public RentBookItemStatus Status { get; set; } = RentBookItemStatus.Available;
		[Required]
		public string StatusDescription { get; set; }

		[Required]
        [Range(0, 100)]
        public int Condition { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsHidden { get; set; } = false;

        public RentBook? RentBook { get; set; }
    }
}
