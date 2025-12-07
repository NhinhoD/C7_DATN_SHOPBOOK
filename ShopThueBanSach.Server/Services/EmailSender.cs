using System.Net.Mail;
using System.Net;
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
            try
            {
                var smtpSection = _configuration.GetSection("Smtp");

                // Lấy config từ Environment (Render sẽ tự map Smtp__Host vào smtpSection["Host"])
                var host = smtpSection["Host"];
                var port = int.Parse(smtpSection["Port"]!);
                var username = smtpSection["Username"];
                var password = smtpSection["Password"];

                Console.WriteLine($"[OLD CODE] Connecting to {host}:{port} using System.Net.Mail...");

                using var client = new SmtpClient(host)
                {
                    Port = port,
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true,
                    Timeout = 30000 // Thêm timeout 30s để tránh treo mãi mãi
                };

                var message = new MailMessage
                {
                    From = new MailAddress(username!),
                    Subject = subject,
                    Body = htmlContent,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                Console.WriteLine("[OLD CODE] Gửi thành công!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OLD CODE ERROR] {ex.ToString()}");
                throw;
            }
        }
    }
}