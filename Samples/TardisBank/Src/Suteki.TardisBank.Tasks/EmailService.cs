namespace Suteki.TardisBank.Tasks
{
    using System;
    using System.Net;
    using System.Net.Mail;

    public interface IEmailService
    {
        void SendEmail(string toAddress, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        readonly TardisConfiguration _configuration;

        public EmailService(TardisConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(string toAddress, string subject, string body)
        {
            if (toAddress == null)
            {
                throw new ArgumentNullException(nameof(toAddress));
            }
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }
            if (string.IsNullOrWhiteSpace(_configuration.EmailSmtpServer)) return;

            var message = new MailMessage(
                        _configuration.EmailFromAddress,
                        toAddress,
                        subject,
                        body);

            var client = new SmtpClient(_configuration.EmailSmtpServer)
            {
                EnableSsl = _configuration.EmailEnableSsl,
                Port = _configuration.EmailPort,
            };

            if (!string.IsNullOrWhiteSpace(_configuration.EmailCredentialsUserName) && 
                !string.IsNullOrWhiteSpace(_configuration.EmailCredentialsPassword))
            {
                client.Credentials = new NetworkCredential(_configuration.EmailCredentialsUserName,
                                                           _configuration.EmailCredentialsPassword);
            }

            client.Send(message);            
        }
    }
}