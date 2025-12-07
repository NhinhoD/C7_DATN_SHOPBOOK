using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ShopThueBanSach.Server.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;

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
            // 1. CẤU HÌNH (Lấy từ Environment trên Render)
            // Lưu ý: Key phải là Smtp__Username (2 gạch dưới)
            var smtpUser = _configuration["Smtp:Username"];
            var smtpPass = _configuration["Smtp:Password"];
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 587; // <--- ĐỔI VỀ 587

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Shop Sach Online", smtpUser));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlContent };
            email.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Tăng timeout lên 30s để chờ mạng
                client.Timeout = 30000;

                // Debug: Ghi log để biết nó đang chạy đến đâu
                Console.WriteLine($"[MAIL DEBUG] Connecting to {smtpHost}:{smtpPort}...");

                // 2. KẾT NỐI: Dùng Port 587 + StartTls
                // (MailKit xử lý StartTls tốt hơn System.Net.Mail cũ rất nhiều)
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

                Console.WriteLine("[MAIL DEBUG] Authenticating...");
                // 3. ĐĂNG NHẬP
                await client.AuthenticateAsync(smtpUser, smtpPass);

                Console.WriteLine("[MAIL DEBUG] Sending...");
                // 4. GỬI
                await client.SendAsync(email);

                Console.WriteLine("[MAIL DEBUG] Sent Successfully!");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết
                Console.WriteLine($"[EMAIL ERROR] Lỗi chi tiết: {ex.ToString()}");
                throw;
            }
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(true);
            }
        }
    }
}