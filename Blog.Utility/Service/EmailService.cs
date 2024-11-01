using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Blog.Utility.Service.IService;
using Blog.Models.Models;
using Blog.Models.Dto;
using Blog.DataAccess.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;


namespace Blog.Utility.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailService(IOptions<EmailSettings> emailSettings, ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _emailSettings = emailSettings.Value;
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public void SendEmail(EmailDto request)
        {
            var email = CreateEmailMessage(request);
            Send(email, request.To);
        }

        private MimeMessage CreateEmailMessage(EmailDto request)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            emailMessage.To.Add(MailboxAddress.Parse(request.To));
            emailMessage.Subject = request.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = request.Body };

            return emailMessage;
        }

        private async void Send(MimeMessage mailMessage, string sendEmail)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(_emailSettings.EmailHost, _emailSettings.EmailPort, SecureSocketOptions.StartTls);
                    client.Authenticate(_emailSettings.EmailUsername, "vlzp twlj jmeu sqco");
                    client.Send(mailMessage);
                }
                catch
                {
                    var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                    var faildEmail = new FailedToSendEmail
                    {
                        Category = "Payment",
                        Email = sendEmail,
                        UserId = userId
                    };
                   await _db.FailedToSendEmails.AddAsync(faildEmail);
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
    }
}
