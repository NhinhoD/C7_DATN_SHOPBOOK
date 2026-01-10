
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopThueBanSach.Server.Area.Admin.Service;
using ShopThueBanSach.Server.Area.Admin.Service.Interface;
using ShopThueBanSach.Server.Data;
using ShopThueBanSach.Server.Entities;
using ShopThueBanSach.Server.Models;
using ShopThueBanSach.Server.Models.PaymentMethod;
using ShopThueBanSach.Server.Services;
using ShopThueBanSach.Server.Services.Interfaces;
using ShopThueBanSach.Server.Services.Vnpay;
using System.Security.Claims;
using System.Text;

namespace ShopThueBanSach.Server
{
	public class Program
	{
		public static async Task Main(string[] args)
		{

            //AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddDistributedMemoryCache();
			builder.Services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromHours(2);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
				options.Cookie.SameSite = SameSiteMode.None;
				options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
			}); // Thêm bộ nhớ đệm cho Session
			builder.Services.AddHttpContextAccessor(); // cần để lấy Session
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new() { Title = "Identity API", Version = "v1" });

				// Thêm cấu hình security definition
				c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
				{
					Description = @"JWT Authorization header using the Bearer scheme.
                        Enter 'Bearer' [space] and then your token in the text input below.
                        Example: 'Bearer eyJhbGciOiJIUzI1NiIs...'",
					Name = "Authorization",
					In = Microsoft.OpenApi.Models.ParameterLocation.Header,
					Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
					Scheme = "Bearer"
				});

				c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
				{
					{
					new Microsoft.OpenApi.Models.OpenApiSecurityScheme
					{
						Reference = new Microsoft.OpenApi.Models.OpenApiReference
						{
							Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
							Id = "Bearer"
						},
							Scheme = "oauth2",
							Name = "Bearer",
In = Microsoft.OpenApi.Models.ParameterLocation.Header,

					},
							new List<string>()
					}
				});
			});
			builder.Services.AddControllers()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.PropertyNamingPolicy = null;
				});
			// Add services to the container.
			builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
			builder.Services.AddControllers();
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddTransient<IEmailSender, EmailSender>();
			builder.Services.AddScoped<ICustomerService, CustomerService>();
			builder.Services.AddScoped<IStaffService, StaffService>();
			builder.Services.AddScoped<IAuthorService, AuthorService>();
			builder.Services.AddScoped<ICategoryService, CategoryService>();
			builder.Services.AddScoped<IRentBookService, RentBookService>();
			builder.Services.AddScoped<ISaleBookService, SaleBookService>();
			builder.Services.AddScoped<IRentBookItemService, RentBookItemService>();
			builder.Services.AddScoped<IReportService, ReportService>();
			builder.Services.AddScoped<IActivityNotificationService, ActivityNotificationService>();
			builder.Services.AddScoped<IUserManagerService, UserManagerService>();
			builder.Services.AddScoped<ISlideService, SlideService>();
			builder.Services.AddScoped<IDiscountCodeService, DiscountCodeService>();
			builder.Services.AddScoped<IPromotionService, PromotionService>();
			builder.Services.AddScoped<IVoucherService, VoucherService>();
			builder.Services.AddScoped<IFavoriteBookService, FavoriteBookService>();
			builder.Services.AddScoped<ICartRentService, CartRentService>();
			builder.Services.AddScoped<IRentOrderService, RentOrderService>();
			builder.Services.Configure<MomoConfig>(builder.Configuration.GetSection("Momo"));
			builder.Services.AddScoped<IMoMoPaymentService, MoMoPaymentService>();
			builder.Services.AddScoped<IMoMoCallbackService, MoMoCallbackService>();
			builder.Services.AddScoped<IRoleService, RoleService>();
			builder.Services.AddScoped<IFavoriteRentBookService, FavoriteRentBookService>();
			builder.Services.AddScoped<ICommentService, CommentService>();
			builder.Services.AddScoped<ISaleCartService, SaleCartService>();
			builder.Services.AddScoped<ISaleOrderService, SaleOrderService>();
			builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();
			builder.Services.AddScoped<ISaleOrderManagementService, SaleOrderManagementService>();
			builder.Services.AddScoped<IPhotoService, PhotoService>();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				options.OperationFilter<SwaggerFileOperationFilter>(); // Hỗ trợ IFormFile
			});
			builder.Services.AddHttpContextAccessor();
			builder.Services.AddMemoryCache();
			builder.Services.AddDbContext<AppDBContext>(options =>
				options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
			builder.Services.AddIdentity<User, IdentityRole>()
			.AddEntityFrameworkStores<AppDBContext>()
			.AddDefaultTokenProviders();

			builder.Services.Configure<IdentityOptions>(options =>
			{
				options.User.AllowedUserNameCharacters =
					"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@ +đĐăĂâÂêÊôÔơƠưƯáàạảãấầậẩẫắằặẳẵéèẹẻẽếềệểễíìịỉĩóòọỏõốồộổỗớờợởỡúùụủũứừựửữýỳỵỷỹÁÀẠẢÃẤẦẬẨẪẮẰẶẲẴÉÈẸẺẼẾỀỆỂỄÍÌỊỈĨÓÒỌỎÕỐỒỘỔỖỚỜỢỞỠÚÙỤỦŨỨỪỰỬỮÝỲỴỶỸ ";
			});

			// Add JWT Authentication
			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
			}).AddCookie().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
			{
				options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
				options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
			}).AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
			{
				options.AppId = builder.Configuration["Authentication:Facebook:AppId"]!;
				options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"]!;
				options.Scope.Add("email");
				options.Fields.Add("email");
				options.Fields.Add("name");
			})
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = builder.Configuration["Jwt:Issuer"],
					ValidAudience = builder.Configuration["Jwt:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
					RoleClaimType = ClaimTypes.Role
				};
			});
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policyBuilder => 
                {
                    policyBuilder.WithOrigins(
                            "https://www.shopbookhexa.io.vn",
                            "https://shopbookhexa.io.vn",
                            "http://localhost:3000",
                            "http://localhost:5173"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            builder.Services.AddScoped<IVnPayService, VnPayService>();
			var app = builder.Build();

			app.UseDefaultFiles();
			app.UseStaticFiles();


			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "DATN API V1");
					c.RoutePrefix = "swagger";
				});
			}
			//SeedData
		
			app.UseHttpsRedirection();
			app.UseCors("AllowFrontend");
			app.UseSession();
			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllers();

			app.MapFallbackToFile("/index.html");

			app.Run();
		}
	}
}