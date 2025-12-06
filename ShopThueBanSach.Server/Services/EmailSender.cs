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

            var smtpSection = _configuration.GetSection("Smtp");

            using var client = new SmtpClient(smtpSection["Host"]!)
            {
                Port = int.Parse(smtpSection["Port"]!),
                // THÊM DÒNG NÀY VÀO TRƯỚC Credentials
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpSection["Username"]!, smtpSection["Password"]!),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(smtpSection["Username"]!),
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
