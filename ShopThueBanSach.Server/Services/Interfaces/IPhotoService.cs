namespace ShopThueBanSach.Server.Services.Interfaces
{
	public interface IPhotoService
	{
		Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file, string folderName);

		Task<bool> DeleteImageAsync(string publicId);
	}
}