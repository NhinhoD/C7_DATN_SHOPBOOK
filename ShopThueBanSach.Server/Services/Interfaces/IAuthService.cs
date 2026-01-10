using Microsoft.AspNetCore.Authentication;
using ShopThueBanSach.Server.Models.AuthModel;

namespace ShopThueBanSach.Server.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterDto model);
        Task<AuthResult> LoginAsync(LoginDto model);
        Task<AuthResult> RefreshTokenAsync(TokenRequestDto model);
        Task<AuthResult> SendEmailConfirmationCodeAsync(string email);
        Task<AuthResult> ConfirmEmailByCodeAsync(string email, string code);
        Task<AuthResult> SendResetPasswordCodeAsync(string email);
        Task<AuthResult> ResetPasswordAsync(ResetPasswordDto model);
		#region login Google
		AuthenticationProperties GetGoogleLoginProperties(string returnUrl);
		Task<AuthResult> ExternalLoginCallbackAsync();
		#endregion
	}
}
