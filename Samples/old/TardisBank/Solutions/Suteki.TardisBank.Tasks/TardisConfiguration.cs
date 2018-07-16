namespace Suteki.TardisBank.Tasks
{
    public class TardisConfiguration
    {
        public TardisConfiguration()
        {
            // set some useful defaults here
            this.ScheduleKey = "run";

            this.EmailSmtpServer = ""; // if this string is empty emails just don't get sent
            this.EmailEnableSsl = false;
            this.EmailPort = 25;
            this.EmailCredentialsUserName = ""; // if the username or password is blank, default credentials are used.
            this.EmailCredentialsPassword = "";
            this.EmailFromAddress = "";

            this.GoogleAnalyticsUaCode = "";
        }

        public string ScheduleKey { get; set; }

        public string EmailSmtpServer { get; set; }
        public bool EmailEnableSsl { get; set; }
        public int EmailPort { get; set; }
        public string EmailCredentialsUserName { get; set; }
        public string EmailCredentialsPassword { get; set; }
        public string EmailFromAddress { get; set; }

        public string GoogleAnalyticsUaCode { get; set; }
    }
}