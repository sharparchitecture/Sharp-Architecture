// ReSharper disable InconsistentNaming

 // see this blog post for sending emails by gmail/google apps
// http://mikehadlow.blogspot.com/2010/08/how-to-send-email-via-gmail-using-net.html

// ReSharper disable PublicMembersMustHaveComments
namespace Suteki.TardisBank.Tests.Services
{
    using global::Suteki.TardisBank.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class EmailServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            var configuration = new TardisConfiguration
            {
                EmailSmtpServer = "smtp.gmail.com",
                EmailPort = 587,
                EmailEnableSsl = true,
                EmailFromAddress = "info@tardisbank.com",
                EmailCredentialsUserName = "____",
                EmailCredentialsPassword = "____"
            };

            emailService = new EmailService(configuration);
        }

        IEmailService emailService;

        [Test, Explicit("You should setup the configuration above. This test really does send an email!")]
        public void Should_be_able_to_send_an_email()
        {
            const string toAddress = "mike@suteki.co.uk";
            const string subject = "Hello From Tardis Bank!";
            const string body = "This is a body http://tardisbank.com/";

            emailService.SendEmail(toAddress, subject, body);
        }
    }
}

// ReSharper restore InconsistentNaming
