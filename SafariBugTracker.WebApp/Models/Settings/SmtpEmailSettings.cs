namespace SafariBugTracker.WebApp.Models.Settings
{
    /// <summary>
    /// Defines all the settings required to send an SMTP email using AWS Simple email service
    /// </summary>
    public class SmtpEmailSettings
    {

        /// <summary>
        /// Email address of where to send the email
        /// </summary>
        public string ToAddress     { get; set; }

        /// <summary>
        /// Email address of where the message was sent from
        /// </summary>
        public string FromAddress   { get; set; }

        /// <summary>
        /// Name of the account or user who sent the message
        /// </summary>
        public string FromName      { get; set; }

        /// <summary>
        /// Credentials for the account used to send the email over SMTP
        /// </summary>
        public string SmtpUserName  { get; set; }

        /// <summary>
        /// Credentials for the account used to send the email over SMTP
        /// </summary>
        public string SmtpPassword  { get; set; }

        /// <summary>
        /// AWS SES region in the form of email-smtp.{region}.amazonaws.com
        /// For a list of supported regions, see:
        /// https://docs.aws.amazon.com/general/latest/gr/rande.html#ses_region
        /// https://docs.aws.amazon.com/ses/latest/DeveloperGuide/send-using-smtp-net.html
        /// </summary>
        public string Host          { get; set; }

        /// <summary>
        /// Port of the SMTP server used to send the emails
        /// </summary>
        public string Port          { get; set; }
    }
}