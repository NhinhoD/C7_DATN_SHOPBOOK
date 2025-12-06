using MailKit.Net.Smtp;
using ShopThueBanSach.Server.Services.Interfaces;
using MimeKit;

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
            var smtpSection = _configuration.GetSection("Smtp");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Shop Sach Online", smtpSection["Username"]));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = htmlContent;
            email.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Kết nối tới Gmail (dùng Port 587 với SecureSocketOptions.StartTls)
                await client.ConnectAsync(smtpSection["Host"], int.Parse(smtpSection["Port"]!), MailKit.Security.SecureSocketOptions.StartTls);

                // Đăng nhập
                await client.AuthenticateAsync(smtpSection["Username"], smtpSection["Password"]);

                // Gửi mail
                await client.SendAsync(email);
            }
            catch (Exception ex)
            {
                // Log lỗi ra để xem nếu có vấn đề
                System.Console.WriteLine("Lỗi gửi mail: " + ex.Message);
                throw;
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}
