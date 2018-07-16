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
        readonly TardisConfiguration configuration;

        public EmailService(TardisConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void SendEmail(string toAddress, string subject, string body)
        {
            if (toAddress == null)
            {
                throw new ArgumentNullException("toAddress");
            }
            if (subject == null)
            {
                throw new ArgumentNullException("subject");
            }
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }
            if (string.IsNullOrWhiteSpace(this.configuration.EmailSmtpServer)) return;

            var message = new MailMessage(
                        this.configuration.EmailFromAddress,
                        toAddress,
                        subject,
                        body);

            var client = new SmtpClient(this.configuration.EmailSmtpServer)
            {
                EnableSsl = this.configuration.EmailEnableSsl,
                Port = this.configuration.EmailPort,
            };

            if (!string.IsNullOrWhiteSpace(this.configuration.EmailCredentialsUserName) && 
                !string.IsNullOrWhiteSpace(this.configuration.EmailCredentialsPassword))
            {
                client.Credentials = new NetworkCredential(this.configuration.EmailCredentialsUserName,
                                                           this.configuration.EmailCredentialsPassword);
            }

            client.Send(message);            
        }
    }
}