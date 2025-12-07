using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ShopThueBanSach.Server.Services.Interfaces;
namespace ShopThueBanSach.Server.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            // 1. Lấy cấu hình từ Biến môi trường
            var smtpUser = _configuration["Smtp:Username"];
            var smtpPass = _configuration["Smtp:Password"];
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 465;

            // 2. KIỂM TRA AN TOÀN (Quan trọng):
            // Nếu quên set biến môi trường trên Render, code sẽ báo lỗi rõ ràng thay vì lỗi chung chung
            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                throw new Exception("Lỗi Cấu hình: Chưa tìm thấy Smtp:Username hoặc Smtp:Password trong Environment Variables.");
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Shop Sach Online", smtpUser));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = htmlContent;
            email.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Timeout kết nối: 10 giây (để không bị treo quá lâu nếu mạng lag)
                client.Timeout = 100000;

                // 3. KẾT NỐI:
                // Dùng Port 465 + SslOnConnect để vượt qua tường lửa Google trên Render
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.SslOnConnect);

                // 4. ĐĂNG NHẬP
                await client.AuthenticateAsync(smtpUser, smtpPass);

                // 5. GỬI
                await client.SendAsync(email);
            }
            catch (Exception ex)
            {
                // Log lỗi ra màn hình Console của Render (xem trong tab Logs)
                Console.WriteLine($"[EMAIL ERROR] Failed to send email to {toEmail}. Error: {ex.Message}");
                throw; // Ném lỗi ra ngoài để Controller bắt được và báo về Frontend
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}