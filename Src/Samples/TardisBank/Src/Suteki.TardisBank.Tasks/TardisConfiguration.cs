namespace Suteki.TardisBank.Tasks;

public class TardisConfiguration
{
    public string ScheduleKey { get; set; }

    public string EmailSmtpServer { get; set; }
    public bool EmailEnableSsl { get; set; }
    public int EmailPort { get; set; }
    public string EmailCredentialsUserName { get; set; }
    public string EmailCredentialsPassword { get; set; }
    public string EmailFromAddress { get; set; }

    public string GoogleAnalyticsUaCode { get; set; }

    public TardisConfiguration()
    {
        // set some useful defaults here
        ScheduleKey = "run";

        EmailSmtpServer = ""; // if this string is empty emails just don't get sent
        EmailEnableSsl = false;
        EmailPort = 25;
        EmailCredentialsUserName = ""; // if the username or password is blank, default credentials are used.
        EmailCredentialsPassword = "";
        EmailFromAddress = "";

        GoogleAnalyticsUaCode = "";
    }
}
