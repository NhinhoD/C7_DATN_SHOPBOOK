using MailKit.Net.Smtp; // Bắt buộc dùng cái này
using MailKit.Security; // Để dùng SecureSocketOptions
using MimeKit;
using ShopThueBanSach.Server.Services.Interfaces;

namespace ShopThueBanSach.Server.Services
{
    public class EmailSender : IEmailSender // Đảm bảo tên Interface đúng với project của bạn
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            // Lấy cấu hình (Ưu tiên lấy từ Biến môi trường trên Render)
            var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var smtpPort = 465; // CỐ ĐỊNH PORT 465 CHO ỔN ĐỊNH
            var smtpUser = _configuration["Smtp:Username"];
            var smtpPass = _configuration["Smtp:Password"];

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
                // QUAN TRỌNG: Dùng Port 465 và SslOnConnect
                // Đây là chìa khóa để fix lỗi Timeout trên Render
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.SslOnConnect);

                await client.AuthenticateAsync(smtpUser, smtpPass);

                await client.SendAsync(email);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi ra Console của Render để debug
                Console.WriteLine($"[EMAIL ERROR] {ex.Message}");
                throw; // Ném lỗi ra để Controller bắt được và trả về Frontend
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}