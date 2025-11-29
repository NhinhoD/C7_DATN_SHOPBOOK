using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using ShopThueBanSach.Server.Models;
using ShopThueBanSach.Server.Services.Interfaces;
using Npgsql.BackendMessages;

public class PhotoService : IPhotoService
{
	private readonly Cloudinary _cloudinary;

	public PhotoService(IOptions<CloudinarySettings> config)
	{
		var acc = new Account(
			config.Value.CloudName,
			config.Value.ApiKey,
			config.Value.ApiSecret);

		_cloudinary = new Cloudinary(acc);
	}

	// Upload ảnh mới
	public async Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file, string folderName)
	{
		if (file.Length <= 0) return (null, null);

		await using var stream = file.OpenReadStream();
		var uploadParams = new ImageUploadParams
		{
			File = new FileDescription(file.FileName, file.OpenReadStream()),
			Folder = folderName,
			PublicId = Guid.NewGuid().ToString(),
			Overwrite = true
		};

		var uploadResult = await _cloudinary.UploadAsync(uploadParams);

		if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
		{
			return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
		}

		return (null, null);
	}

	// Xóa ảnh cũ
	public async Task<bool> DeleteImageAsync(string publicId)
	{
		var deletionParams = new DeletionParams(publicId);
		var result = await _cloudinary.DestroyAsync(deletionParams);

		return result.Result == "ok";
	}
}