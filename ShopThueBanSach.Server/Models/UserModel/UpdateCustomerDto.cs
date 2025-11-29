namespace ShopThueBanSach.Server.Models.UserModel
{
    public class UpdateCustomerDto
    {
            
        public string? Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public double? Points { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
