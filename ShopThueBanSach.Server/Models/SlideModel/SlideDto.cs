using System.ComponentModel.DataAnnotations;

namespace ShopThueBanSach.Server.Models.SlideModel
{
    public class SlideDto
    {
        [Required]
        public IFormFile ImageFile { get; set; } = null!;
        public string? LinkUrl { get; set; }
    }
}
