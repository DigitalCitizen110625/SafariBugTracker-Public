using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SafariBugTracker.WebApp.Helpers;
using SafariBugTracker.WebApp.Models;
using SafariBugTracker.WebApp.Services;
using static SafariBugTracker.WebApp.Models.UIAlert;
using Log = Serilog.Log;

namespace SafariBugTracker.WebApp.Controllers
{

    /// <summary>
    /// Handles all GET and POST requests to the associated views found on the home page
    /// </summary>
    public class HomeController : Controller
    {
        #region Fields, Properties, Constructor
        
        

        /// <summary>
        /// Logger instance for tracking errors and user actions
        /// </summary>
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Primary service responsible for sending email
        /// </summary>
        private readonly EmailService _emailService;

        public HomeController(ILogger<HomeController> logger, EmailService emailService)
        {
            _logger = logger ?? throw new ArgumentNullException($" {nameof(logger)} cannot be null");
            _emailService = emailService ?? throw new ArgumentNullException($" {nameof(emailService)} cannot be null");
        }



        #endregion
        #region HttpGet



        /// <summary>
        /// Presents the main/home page
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// Presents the privacy page
        /// </summary>
        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }


        /// <summary>
        /// Presents the about page
        /// </summary>
        [HttpGet]
        public IActionResult About()
        {
            return View();
        }


        /// <summary>
        /// Presents the features page
        /// </summary>
        [HttpGet]
        public IActionResult Features()
        {
            return View();
        }


        /// <summary>
        /// Presents the frequently asked questions (FAQ) page
        /// </summary>
        [HttpGet]
        public IActionResult Faq()
        {
            return View();
        }


        /// <summary>
        /// Presents the contact page
        /// </summary>
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




        #endregion
        #region HttpPost


        /// <summary>
        /// Presents the main page on a POST request using the contact us form.
        /// Ensures the message contents is valid before sending the message.
        /// </summary>
        /// <param name="contactUsMessage">ContactMessage model containins the users input from the message form</param>
        /// <returns>View with validation errors, or a confirmation message if the message was sent </returns>
        [HttpPost]
        public async Task<IActionResult> Index(ContactMessage contactUsMessage)
        {
            if (!ModelState.IsValid)
            {
                return View(contactUsMessage);
            }

            string messageTemplate = "Contact Us Message {NEW_LINE}" +
                "Sender Name: {SENDER_NAME} " +
                "Sender Address: {SENDER_ADDRESS} " +
                "Sender Message: {MESSAGE}";

            StringBuilder message = new StringBuilder(messageTemplate);
            message.Replace("{SENDER_NAME}", contactUsMessage.ContactName);
            message.Replace("{SENDER_ADDRESS}", contactUsMessage.ContactEmail);
            message.Replace("{MESSAGE}", contactUsMessage.Message);
            message.Replace("{NEW_LINE}", Environment.NewLine);

            try
            {
                await _emailService.SendEmail(contactUsMessage.ContactName, contactUsMessage.Message, contactUsMessage.ContactEmail);
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Success, "Message sent successfully"));
                Log.Information(message.ToString());
            }
            catch (System.Exception e)
            {
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "Unable to send message, please try again later"));
                Log.Error(e, $"Message: {message.ToString()} caused error: {e.Message}");
            }

            return View();
        }


        #endregion
    }//class
}//namespace