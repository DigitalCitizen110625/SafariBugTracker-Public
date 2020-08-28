/*
 * Date: August 22, 2020
 * Version: 1.0
 * Note: At the moment, all emails are sent using AWS Simple Email Service. Because the account used to send the emails doesn't have production access, 
 *       this application can only send and receive emails from verified accounts. Thus, the sender email has to be included in the message body, and
 *       the FromAddress, and ToAddress are provided in the config, and not set to the senderAddress
 */


using Microsoft.Extensions.Options;
using SafariBugTracker.WebApp.Models.Settings;
using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SafariBugTracker.WebApp.Services
{

    /// <summary>
    /// Primary service used for sending SMTP emails from the application
    /// </summary>
    public class EmailService
    {
        #region Fields, Properties, Constructor

        /// <summary>
        /// The displayed subject line for all sent emails
        /// </summary>
        const string Subject = "Safari Contact Us Message";

        /// <summary>
        /// The email address of where to send the message
        /// </summary>
        string ToAddress { get; set; }

        /// <summary>
        /// Name of the user/account of where the email will be sent
        /// </summary>
        string ToName    { get; set; }

        /// <summary>
        /// Email address from where the email originated
        /// NOTE: Email addresses are case-sensitive. Make sure that the address is exactly the same as the one you intend to send from
        /// </summary>
        string FromAddress { get; set; }

        /// <summary>
        /// Name of the account/user from where the mail originated
        /// </summary>
        string FromName { get; set; }

        /// <summary>
        /// AWS SES SMTP user name
        /// </summary>
        string SmtpUserName { get; set; }

        /// <summary>
        /// AWS SES SMTP password
        /// </summary>
        string SmtpPassword { get; set; }

        /// <summary>
        /// AWS SES host information
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// The port used to connect to on the Amazon SES SMTP endpoint
        /// Port 587 is standard when using STARTTLS to encrypt the connection
        /// </summary>
        int Port { get; set; }


        public EmailService(IOptions<SmtpEmailSettings> emailSettings)
        {
            ToAddress = emailSettings.Value.ToAddress;
            FromName = emailSettings.Value.FromName;
            FromAddress = emailSettings.Value.FromAddress;
            SmtpUserName = emailSettings.Value.SmtpUserName;
            SmtpPassword = emailSettings.Value.SmtpPassword;
            Host = emailSettings.Value.Host;
            Port = int.Parse(emailSettings.Value.Port);
        }



        #endregion
        #region PrivateMethods



        /// <summary>
        /// Incorporates the senders details, and message content into an html template
        /// </summary>
        /// <param name="senderName">Name of the user sending the message</param>
        /// <param name="senderName">Email address of the user sending the message</param>
        /// <param name="message">Content of the message to show in the email</param>
        /// <returns>String containing the html, and message content of the email</returns>
        private string BuildMessageBody(string senderName, string senderAddress, string message)
        {
            string baseBodyTemplate =
                "<h2> Contact Us Message </h2>" +
                "<br/>" +
                "<h5>Sender Name: {SENDER_NAME}</h5>" +
                "<h5>Sender Address: {SENDER_ADDRESS}</h5>" +
                "<p>{MESSAGE}</p>" +
                "<hr/>" +
                "<small>This is an automated email sent from the SafariBugTracker Web Application</small>";

            StringBuilder body = new StringBuilder(baseBodyTemplate);
            body.Replace("{SENDER_NAME}", senderName);
            body.Replace("{MESSAGE}", message);
            body.Replace("{SENDER_ADDRESS}", senderAddress);

            return body.ToString();
        }



        #endregion
        #region PublicMethods



        /// <summary>
        /// Creates the mail message, and sends it over SMTP to the target address
        /// For more information on sending SMTP emails using AWS Simple Email Service,
        /// see: https://docs.aws.amazon.com/ses/latest/DeveloperGuide/send-using-smtp-net.html
        /// </summary>
        /// <param name="senderName">Name of the user sending the message</param>
        /// <param name="messageContent">The content of the message shown in the email body</param>
        /// <param name="toAddress">Email address where the email will be sent</param>
        /// <remarks>Throws the following exceptions: 
        ///     • ArgumentNullException
        ///     • ObjectDisposedException
        ///     • SmtpException
        ///     • SmtpFailedRecipientException
        ///     • SmtpFailedRecipientsException
        ///     
        /// See SmtpClient.Send for the cause of each exception
        /// </remarks>
        public async Task SendEmail(string senderName, string messageContent, string senderAddress)
        {
            //Ensure all fields are valid before building the email
            if (string.IsNullOrEmpty(senderName))
            {
                throw new ArgumentNullException(nameof(senderName) + " cannot be null");
            }
            if (string.IsNullOrEmpty(messageContent))
            {
                throw new ArgumentNullException(nameof(messageContent) + " cannot be null");
            }
            if (string.IsNullOrEmpty(senderAddress))
            {
                throw new ArgumentNullException(nameof(senderAddress) + " cannot be null");
            }

            //Build the email message
            MailMessage message = new MailMessage();
            message.From = new MailAddress(FromAddress, FromName);
            message.To.Add(new MailAddress(ToAddress));

            message.SubjectEncoding = System.Text.Encoding.UTF8;
            message.Subject = Subject;

            message.IsBodyHtml = true;
            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.Body = BuildMessageBody(senderName, senderAddress, messageContent);

            //Configure the SMTP properties and send the message
            using (var client = new System.Net.Mail.SmtpClient(Host, Port))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(SmtpUserName, SmtpPassword);
                client.EnableSsl = true;

                await client.SendMailAsync(message);
            }
        }



        #endregion
    }//class
}//namespace