using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SafariBugTracker.WebApp.Models;

namespace SafariBugTracker.WebApp.Helpers
{

    /// <summary>
    /// Responsible for passing alert notifications to the view
    /// </summary>
    public static class UIAlertHelper
    {

        /// <summary>
        /// Sets the alert notification in the ViewData, to the passed in UIAlert
        /// </summary>
        /// <param name="controller">The calling controller from where the alert is set</param>
        /// <param name="alert">UIAlert model containing the alert type, and message</param>
        public static void SetAlertNotification(Controller controller, UIAlert alert)
        {
            controller.TempData["AlertNotification"] = JsonConvert.SerializeObject(alert);
        }

    }//class
}//namespace