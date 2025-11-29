using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models.AuthModel;
using ShopThueBanSach.Server.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShopThueBanSach.Server.Services
{
	public class AuthService(UserManager<User> userManager, AppDBContext context, RoleManager<IdentityRole> roleManager,
		IConfiguration configuration, SignInManager<User> signInManager, IMemoryCache cache, IHttpContextAccessor httpContextAccessor, IEmailSender emailSender, IActivityNotificationService activityNotificationService) : IAuthService
	{
		private readonly UserManager<User> _userManager = userManager;
		private readonly IConfiguration _configuration = configuration;
		private readonly SignInManager<User> _signInManager = signInManager;
		private readonly RoleManager<IdentityRole> _roleManager = roleManager;
		private readonly AppDBContext _context = context;
		private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
		private readonly IEmailSender _emailSender = emailSender;
		private readonly IMemoryCache _cache = cache;
		private readonly IActivityNotificationService _activityNotificationService;


		public AuthenticationProperties GetFacebookLoginProperties(string returnUrl)
		{
			var redirectUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/api/Auth/facebook-callback?returnUrl={returnUrl}";
			return _signInManager.ConfigureExternalAuthenticationProperties("Facebook", redirectUrl);
		}

		public async Task<AuthResult> ExternalFacebookCallbackAsync()
		{
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
				return new AuthResult { IsSuccess = false, Message = "Không thể lấy thông tin từ Facebook." };

			// Đăng nhập nếu đã có liên kết login
			var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
			if (signInResult.Succeeded)
			{
				var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
				if (user != null)
				{
					var jwtToken = GenerateJwtToken(user);
					var refreshToken = GenerateRefreshToken();

					_httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Strict,
						Expires = DateTimeOffset.UtcNow.AddDays(7)
					});

					return new AuthResult
					{
						IsSuccess = true,
						Token = jwtToken,
						RefreshToken = refreshToken,
						Message = "Đăng nhập bằng Facebook thành công."
					};
				}
			}

			// Nếu user chưa tồn tại, tạo mới
			var email = info.Principal.FindFirstValue(ClaimTypes.Email);
			var name = info.Principal.FindFirstValue(ClaimTypes.Name);

			var existingUser = await _userManager.FindByEmailAsync(email!);
			if (existingUser == null)
			{
				var newUser = new User
				{
					UserName = RemoveDiacritics(name.Replace(" ", "").ToLowerInvariant()), // đảm bảo hợp lệ
					Email = email,
					EmailConfirmed = true,
					Address = "",
					DateOfBirth = null, // hoặc DateTime.UtcNow nếu bạn muốn tạm thời
					Points = 0,
					ImageUser = null,
				};

				var createResult = await _userManager.CreateAsync(newUser);
				if (!createResult.Succeeded)
				{
					var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
					return new AuthResult { IsSuccess = false, Message = errors };
				}

				// Gán role
				var roleName = "Customer";
				if (!await _roleManager.RoleExistsAsync(roleName))
					await _roleManager.CreateAsync(new IdentityRole(roleName));
				await _userManager.AddToRoleAsync(newUser, roleName);

				// Gắn thông tin đăng nhập Facebook
				await _userManager.AddLoginAsync(newUser, info);

				var token = GenerateJwtToken(newUser);
				var refresh = GenerateRefreshToken();
				_httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refresh, new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.Strict,
					Expires = DateTimeOffset.UtcNow.AddDays(7)
				});

				return new AuthResult
				{
					IsSuccess = true,
					Token = token,
					RefreshToken = refresh,
					Message = "Tài khoản Facebook đã được đăng ký mới thành công."
				};
			}

			// Nếu user đã tồn tại email nhưng chưa liên kết login
			await _userManager.AddLoginAsync(existingUser, info);

			var existingToken = GenerateJwtToken(existingUser);
			var newRefresh = GenerateRefreshToken();

			_httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", newRefresh, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTimeOffset.UtcNow.AddDays(7)
			});

			return new AuthResult
			{
				IsSuccess = true,
				Token = existingToken,
				RefreshToken = newRefresh,
				Message = "Liên kết Facebook thành công."
			};
		}

		private string RemoveDiacritics(string text)
		{
			var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
			var sb = new StringBuilder();

			foreach (var ch in normalized)
			{
				var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
				if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
				{
					sb.Append(ch);
				}
			}

			return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
		}

		public AuthenticationProperties GetGoogleLoginProperties(string returnUrl)
		{
			var redirectUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/api/Auth/external-login-callback?returnUrl={returnUrl}";
			return _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
		}
		public async Task<AuthResult> ExternalLoginCallbackAsync()
		{
			var info = await _signInManager.GetExternalLoginInfoAsync();
			if (info == null)
				return new AuthResult { IsSuccess = false, Message = "Không thể lấy thông tin từ Google." };

			var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
			if (signInResult.Succeeded)
			{
				var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
				if (user != null)
				{
					var jwtToken = GenerateJwtToken(user);
					var refreshToken = GenerateRefreshToken();

					_httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Strict,
						Expires = DateTimeOffset.UtcNow.AddDays(7)
					});

					return new AuthResult
					{
						IsSuccess = true,
						Token = jwtToken,
						RefreshToken = refreshToken,
						Message = "Đăng nhập bằng Google thành công."
					};
				}
			}

			var email = info.Principal.FindFirstValue(ClaimTypes.Email);
			var name = info.Principal.FindFirstValue(ClaimTypes.Name);

			var existingUser = await _userManager.FindByEmailAsync(email);
			if (existingUser == null)
			{
				var newUser = new User
				{
					UserName = name,
					Email = email,
					EmailConfirmed = true
				};
				var result = await _userManager.CreateAsync(newUser);
				if (!result.Succeeded)
					return new AuthResult { IsSuccess = false, Message = string.Join("; ", result.Errors.Select(e => e.Description)) };

				await _userManager.AddToRoleAsync(newUser, "Customer");
				await _userManager.AddLoginAsync(newUser, info);

				var jwt = GenerateJwtToken(newUser);
				var refresh = GenerateRefreshToken();
				_httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", refresh, new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.Strict,
					Expires = DateTimeOffset.UtcNow.AddDays(7)
				});

				return new AuthResult { IsSuccess = true, Token = jwt, RefreshToken = refresh, Message = "Tạo tài khoản từ Google thành công." };
			}

			// Đã tồn tại email, gán login Google nếu chưa
			await _userManager.AddLoginAsync(existingUser, info);

			var jwtTokenExisting = GenerateJwtToken(existingUser);
			var newRefresh = GenerateRefreshToken();
			_httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", newRefresh, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTimeOffset.UtcNow.AddDays(7)
			});

			return new AuthResult
			{
				IsSuccess = true,
				Token = jwtTokenExisting,
				RefreshToken = newRefresh,
				Message = "Liên kết Google thành công."
			};
		}
		public async Task<AuthResult> RegisterAsync(RegisterDto dto)
		{
			// Kiểm tra Email đã tồn tại chưa
			var existingUser = await _userManager.FindByEmailAsync(dto.Email);
			if (existingUser != null)
			{
				return new AuthResult
				{
					IsSuccess = false,
					Message = "Email đã tồn tại."
				};
			}

			var user = new User
			{
				UserName = dto.UserName,
				Email = dto.Email,
				Address = dto.Address,
				DateOfBirth = dto.DateOfBirth.HasValue
	? DateTime.SpecifyKind(dto.DateOfBirth.Value, DateTimeKind.Utc)
	: null,
				EmailConfirmed = false
			};

			var result = await _userManager.CreateAsync(user, dto.Password);
			if (!result.Succeeded)
			{
				var errors = string.Join("; ", result.Errors.Select(e => e.Description));
				return new AuthResult { IsSuccess = false, Message = errors };
			}

			// ✅ Gán Role "Customer"
			var roleName = "Customer";
			if (!await _roleManager.RoleExistsAsync(roleName))
			{
				await _roleManager.CreateAsync(new IdentityRole(roleName));
			}
			await _userManager.AddToRoleAsync(user, roleName);

			// Gửi mã xác nhận email
			var code = GenerateRandomCode(6);
			_cache.Set($"email_confirm_{user.Email}", code, TimeSpan.FromMinutes(10));

			await _emailSender.SendEmailAsync(user.Email, "Xác nhận địa chỉ email của bạn",
$@"
<div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
    <h2 style='color: #2a8dd2;'>Xác nhận địa chỉ Email</h2>
    <p>Xin chào <strong>{user.UserName ?? "bạn"}</strong>,</p>
    <p>Bạn vừa yêu cầu xác nhận địa chỉ email của mình. Vui lòng sử dụng mã xác nhận dưới đây để hoàn tất quá trình:</p>
    <p style='font-size: 18px; margin: 20px 0;'>
        <strong style='background-color: #f0f0f0; padding: 10px 20px; border-radius: 5px; display: inline-block; letter-spacing: 2px;'>{code}</strong>
    </p>
    <p><i>Lưu ý: Mã xác nhận này sẽ hết hạn sau 10 phút.</i></p>
    <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
    <hr />
    <p style='font-size: 14px; color: #999;'>Trân trọng,<br/>Đội ngũ hỗ trợ Customer</p>
</div>");

			return new AuthResult
			{
				IsSuccess = true,
				Message = "Đăng ký thành công. Vui lòng kiểm tra email để xác nhận tài khoản."
			};
		}




		public async Task<AuthResult> LoginAsync(LoginDto model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
				return new AuthResult { IsSuccess = false, Message = "Email hoặc mật khẩu không đúng." };

			if (!user.EmailConfirmed)
				return new AuthResult { IsSuccess = false, Message = "Vui lòng xác nhận email trước khi đăng nhập." };

			var jwtToken = GenerateJwtToken(user);
			var refreshToken = GenerateRefreshToken();

			_httpContextAccessor.HttpContext?.Response.Cookies.Append(
				"refreshToken",
				refreshToken,
				new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.Strict,
					Expires = DateTimeOffset.UtcNow.AddDays(7)
				});

			// ✅ Gửi thông báo nếu là Staff đăng nhập
			var roles = await _userManager.GetRolesAsync(user);
			if (roles.Contains("Staff"))
			{
				var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == user.Email);
				if (staff != null)
				{
					var description = $"Nhân viên {staff.FullName} đã đăng nhập vào hệ thống lúc {DateTime.UtcNow:HH:mm dd/MM/yyyy}.";
					var notification = new ActivityNotification
					{
						NotificationId = Guid.NewGuid().ToString(),
						StaffId = staff.StaffId,
						Description = description,
						CreatedDate = DateTime.UtcNow
					};

					_context.ActivityNotifications.Add(notification);
					await _context.SaveChangesAsync();
				}
			}

			return new AuthResult
			{
				IsSuccess = true,
				Token = jwtToken,
				RefreshToken = refreshToken,
				Message = "Đăng nhập thành công."
			};
		}

		public async Task<AuthResult> RefreshTokenAsync(TokenRequestDto model)
		{
			var principal = GetPrincipalFromExpiredToken(model.AccessToken);
			if (principal == null)
				return new AuthResult { IsSuccess = false, Message = "Token không hợp lệ." };

			var username = principal.Identity?.Name;
			var user = await _userManager.FindByNameAsync(username);

			if (user == null)
				return new AuthResult { IsSuccess = false, Message = "Người dùng không tồn tại." };
			string savedRefreshToken = null!;
			_httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue("refreshToken", out savedRefreshToken!);

			if (string.IsNullOrEmpty(savedRefreshToken) || savedRefreshToken != model.RefreshToken)
			{
				return new AuthResult { IsSuccess = false, Message = "Refresh token không hợp lệ." };
			}

			// Tạo mới token và refresh token
			var newToken = GenerateJwtToken(user);
			var newRefreshToken = GenerateRefreshToken();

			// Lưu refreshToken mới vào cookie
			_httpContextAccessor.HttpContext?.Response.Cookies.Append(
				"refreshToken",
				newRefreshToken,
				new CookieOptions
				{
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.Strict,
					Expires = DateTimeOffset.UtcNow.AddDays(7)
				});

			return new AuthResult
			{
				IsSuccess = true,
				Token = newToken,
				RefreshToken = newRefreshToken,
				Message = "Làm mới token thành công."
			};
		}



		public async Task<string> ConfirmEmailAsync(string userId, string token)
		{
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
				return "<h2>❌ Không tìm thấy người dùng.</h2>";

			if (user.EmailConfirmed)
				return "<h2>✅ Email đã được xác nhận trước đó.</h2>";

			var result = await _userManager.ConfirmEmailAsync(user, token);
			if (!result.Succeeded)
			{
				var errors = string.Join("<br>", result.Errors.Select(e => e.Description));
				return $"<h2>❌ Xác nhận thất bại:</h2><p>{errors}</p>";
			}

			return "<h2>✅ Email của bạn đã được xác nhận thành công!</h2>";
		}

		// ------------------ Private Helpers ------------------

		private string GenerateJwtToken(User user)
		{
			var claims = new List<Claim>
	{
		new Claim(ClaimTypes.NameIdentifier, user.Id),
		new Claim(ClaimTypes.Name, user.UserName!),
		new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim("UserId", user.Id)
	};

			// 🟢 Thêm Role vào Claims
			var roles = _userManager.GetRolesAsync(user).Result;
			foreach (var role in roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
expires: DateTime.UtcNow.AddMinutes(30),
				claims: claims,
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}


		private string GenerateRefreshToken()
		{
			var randomBytes = new byte[64];
			using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
			rng.GetBytes(randomBytes);
			return Convert.ToBase64String(randomBytes);
		}

		private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
		{
			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateAudience = true,
				ValidateIssuer = true,
				ValidAudience = _configuration["Jwt:Audience"],
				ValidIssuer = _configuration["Jwt:Issuer"],
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
				ValidateLifetime = false // Bỏ kiểm tra hết hạn để lấy claim
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			try
			{
				var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
				if (securityToken is not JwtSecurityToken jwtToken ||
					!jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
					return null;

				return principal;
			}
			catch
			{
				return null;
			}
		}

		public async Task<AuthResult> SendResetPasswordCodeAsync(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
				return new AuthResult { IsSuccess = false, Message = "Email không tồn tại." };

			var code = GenerateRandomCode(5); // 5 chữ số
			_cache.Set(email, code, TimeSpan.FromMinutes(5)); // lưu mã vào cache

			await _emailSender.SendEmailAsync(email, "Mã khôi phục mật khẩu", $"Mã của bạn là: {code}");

			return new AuthResult { IsSuccess = true, Message = "Mã đặt lại mật khẩu đã được gửi đến email." };
		}

		public async Task<AuthResult> ResetPasswordAsync(ResetPasswordDto model)
		{
			var user = await _userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return new AuthResult { IsSuccess = false, Message = "Không tìm thấy người dùng." };

			if (!_cache.TryGetValue(model.Email, out string? cachedCode) || cachedCode != model.Code)
			{
				return new AuthResult { IsSuccess = false, Message = "Mã không hợp lệ hoặc đã hết hạn." };
			}

			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
			if (!result.Succeeded)
			{
				var errors = string.Join("; ", result.Errors.Select(e => e.Description));
				return new AuthResult { IsSuccess = false, Message = errors };
			}

			_cache.Remove(model.Email); // Xoá mã sau khi sử dụng
			return new AuthResult { IsSuccess = true, Message = "Đặt lại mật khẩu thành công." };
		}

		private string GenerateRandomCode(int length)
		{
			var random = new Random();
			return string.Join("", Enumerable.Range(0, length).Select(_ => random.Next(0, 10)));
		}

		public async Task<AuthResult> SendEmailConfirmationCodeAsync(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
				return new AuthResult { IsSuccess = false, Message = "Email không tồn tại." };

			if (user.EmailConfirmed)
				return new AuthResult { IsSuccess = false, Message = "Email đã được xác nhận." };

			var code = GenerateRandomCode(6);
			_cache.Set($"email_confirm_{email}", code, TimeSpan.FromMinutes(10));

			await _emailSender.SendEmailAsync(user.Email, "Xác nhận địa chỉ email của bạn",
 $@"
<div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
    <h2 style='color: #2a8dd2;'>Xác nhận địa chỉ Email</h2>
    <p>Xin chào <strong>{user.UserName ?? "bạn"}</strong>,</p>
    <p>Bạn vừa yêu cầu xác nhận địa chỉ email của mình. Vui lòng sử dụng mã xác nhận dưới đây để hoàn tất quá trình:</p>
    <p style='font-size: 18px; margin: 20px 0;'>
        <strong style='background-color: #f0f0f0; padding: 10px 20px; border-radius: 5px; display: inline-block; letter-spacing: 2px;'>{code}</strong>
    </p>
    <p><i>Lưu ý: Mã xác nhận này sẽ hết hạn sau 10 phút.</i></p>
    <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
    <hr />
    <p style='font-size: 14px; color: #999;'>Trân trọng,<br/>Đội ngũ hỗ trợ Customer</p>
</div>");


			return new AuthResult { IsSuccess = true, Message = "Mã xác nhận đã được gửi lại vào email." };
		}


		public async Task<AuthResult> ConfirmEmailByCodeAsync(string email, string code)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
				return new AuthResult { IsSuccess = false, Message = "Email không tồn tại." };

			if (user.EmailConfirmed)
				return new AuthResult { IsSuccess = false, Message = "Email đã được xác nhận trước đó." };

			if (!_cache.TryGetValue($"email_confirm_{email}", out string? cachedCode) || cachedCode != code)
				return new AuthResult { IsSuccess = false, Message = "Mã xác nhận không hợp lệ hoặc đã hết hạn." };

			user.EmailConfirmed = true;
			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				var errors = string.Join("; ", result.Errors.Select(e => e.Description));
				return new AuthResult { IsSuccess = false, Message = errors };
			}

			_cache.Remove($"email_confirm_{email}"); // xoá sau khi dùng
			return new AuthResult { IsSuccess = true, Message = "Xác nhận email thành công." };
		}


	}
}