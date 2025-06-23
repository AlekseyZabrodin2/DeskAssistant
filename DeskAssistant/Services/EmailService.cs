using DeskAssistant.SecureService;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NLog;

namespace DeskAssistant.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings = null;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private EncryptionHelper _encryptionHelper;

        public EmailService(IOptions<EmailSettings> options, EncryptionHelper encryptionHelper)
        {
            _emailSettings = options.Value;
            _encryptionHelper = encryptionHelper;
        }

        public async Task<bool> SendEmailAsync(List<(string nameTo, string emailTo)> addresseeTo, string emailSubject, string emailTextBody)
        {
            string mailSubscriberFrom = _encryptionHelper.Decrypt(_emailSettings.Name);
            string emailIdFrom = _encryptionHelper.Decrypt(_emailSettings.EmailId);
            string decryptedUserName = _encryptionHelper.Decrypt(_emailSettings.Username);
            string decryptedPassword = _encryptionHelper.Decrypt(_emailSettings.Password);

            try
            {
                MimeMessage emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(mailSubscriberFrom, emailIdFrom));

                foreach (var person in addresseeTo)
                {
                    emailMessage.To.Add(new MailboxAddress(person.nameTo, person.emailTo));
                }

                emailMessage.Subject = emailSubject;

                BodyBuilder emailBodyBuilder = new BodyBuilder
                {
                    TextBody = emailTextBody
                };

                emailMessage.Body = emailBodyBuilder.ToMessageBody();

                using var emailClient = new SmtpClient();

                await emailClient.ConnectAsync(_emailSettings.Host, _emailSettings.Port, _emailSettings.UseTLS);
                await emailClient.AuthenticateAsync(decryptedUserName, decryptedPassword);
                await emailClient.SendAsync(emailMessage);
                await emailClient.DisconnectAsync(true);

                _logger.Info("The email has been sent");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Email has not been send - {ex}");
                return false;
            }
        }
    }
}
