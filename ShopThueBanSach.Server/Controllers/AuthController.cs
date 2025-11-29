using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.AuthModel;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly SignInManager<User> _signInManager;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AuthController(
			IAuthService authService,
			SignInManager<User> signInManager,
			IHttpContextAccessor httpContextAccessor)
		{
			_authService = authService;
			_signInManager = signInManager;
			_httpContextAccessor = httpContextAccessor;
		}

		// -------------------- GOOGLE LOGIN --------------------

		[HttpGet("external-login")]
		public IActionResult GetExternalLoginUrl([FromQuery] string provider = "Google", [FromQuery] string returnUrl = "/")
		{
			var loginUrl = $"{Request.Scheme}://{Request.Host}/api/Auth/external-login-redirect?provider={provider}&returnUrl={returnUrl}";
			return Ok(new { loginUrl });
		}

		[HttpGet("external-login-redirect")]
		public IActionResult ExternalLoginRedirect([FromQuery] string provider = "Google", [FromQuery] string returnUrl = "/")
		{
			var redirectUrl = $"{Request.Scheme}://{Request.Host}/api/Auth/external-login-callback?returnUrl={returnUrl}";
			var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
			return Challenge(properties, provider);
		}
		[HttpGet("external-login-callback")]
		public async Task<IActionResult> ExternalLoginCallback([FromQuery] string? returnUrl = "/")
		{
			var result = await _authService.ExternalLoginCallbackAsync();
			if (!result.IsSuccess)
				return Unauthorized(new { message = result.Message });

			// Redirect về frontend với token nếu login thành công
			var redirect = $"https://hexaclovershop.io.vn/oauth-callback?token={result.Token}&refreshToken={result.RefreshToken}";
			return Redirect(redirect);
		}

		// -------------------- FACEBOOK LOGIN --------------------

		[HttpGet("facebook-login")]
		public IActionResult GetFacebookLoginUrl([FromQuery] string returnUrl = "/")
		{
			var loginUrl = $"{Request.Scheme}://{Request.Host}/api/Auth/facebook-login-redirect?returnUrl={returnUrl}";
			return Ok(new { loginUrl });
		}

		[HttpGet("facebook-login-redirect")]
		public IActionResult FacebookLoginRedirect([FromQuery] string returnUrl = "/")
		{
			var props = _authService.GetFacebookLoginProperties(returnUrl);
			return Challenge(props, "Facebook");
		}

		[HttpGet("facebook-callback")]
		public async Task<IActionResult> FacebookCallback([FromQuery] string returnUrl = "/")
		{
			var result = await _authService.ExternalFacebookCallbackAsync();
			return result.IsSuccess ? Ok(result) : Unauthorized(result);
		}
		[HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { message = "Mật khẩu và xác nhận mật khẩu không khớp." });

            var result = await _authService.RegisterAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model);
            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RefreshTokenAsync(model);
            if (!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto model)
        {
            var result = await _authService.ConfirmEmailByCodeAsync(model.Email, model.Code);
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.SendResetPasswordCodeAsync(model.Email);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(model);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto model)
        {
            if (string.IsNullOrEmpty(model.Email))
                return BadRequest(new { message = "Email không được để trống." });

            var result = await _authService.SendEmailConfirmationCodeAsync(model.Email);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Mã OTP đã được gửi lại vào email." });
        }

    }
}
