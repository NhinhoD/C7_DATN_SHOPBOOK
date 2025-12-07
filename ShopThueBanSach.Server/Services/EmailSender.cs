using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ShopThueBanSach.Server.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using System.Net; // Cần thêm dòng này để dùng Dns
using System.Linq; // Cần thêm dòng này để dùng FirstOrDefault

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
            var smtpUser = _configuration["Smtp:Username"];
            var smtpPass = _configuration["Smtp:Password"];
            var host = "smtp.gmail.com";
            var port = 587;

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Shop Sach Online", smtpUser));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlContent };
            email.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                client.Timeout = 30000; // Timeout 10s là đủ

                // --- KỸ THUẬT FORCE IPv4 ---
                // 1. Phân giải tên miền ra IP
                var ipAddresses = await Dns.GetHostAddressesAsync(host);

                // 2. Chỉ lấy địa chỉ IPv4 (InterNetwork)
                var ip4 = ipAddresses.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                if (ip4 != null)
                {
                    Console.WriteLine($"[MAIL DEBUG] Resolved {host} to IPv4: {ip4}");

                    // 3. Bỏ qua lỗi SSL (Vì chứng chỉ Gmail cấp cho domain, không cấp cho IP)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    // 4. Kết nối thẳng vào IP
                    await client.ConnectAsync(ip4.ToString(), port, SecureSocketOptions.StartTls);
                }
                else
                {
                    // Fallback nếu không tìm thấy IPv4
                    await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                }

                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(email);

                Console.WriteLine("[MAIL DEBUG] Gửi thành công!");
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