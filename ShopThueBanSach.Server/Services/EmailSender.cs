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
            // Lấy config từ Environment
            var smtpUser = _configuration["Smtp:Username"];
            var smtpPass = _configuration["Smtp:Password"];

            // QUAY VỀ PORT 465 + SSL (Bây giờ Docker đã tắt IPv6 nên cái này sẽ chạy ngon)
            var smtpHost = "smtp.gmail.com";
            var smtpPort = 465;

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Shop Sach Online", smtpUser));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlContent };
            email.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                client.Timeout = 20000; // 20s timeout

                // LOG DEBUG
                Console.WriteLine($"[MAIL] Connecting to {smtpHost}:{smtpPort} (SSL)...");

                // Dùng SslOnConnect cho Port 465
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.SslOnConnect);

                Console.WriteLine($"[MAIL] Authenticating...");
                await client.AuthenticateAsync(smtpUser, smtpPass);

                Console.WriteLine("[MAIL] Sending...");
                await client.SendAsync(email);

                Console.WriteLine("[MAIL] SUCCESS!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] {ex.Message}");
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}