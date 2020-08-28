namespace SafariBugTracker.WebApp.Models
{
    /// <summary>
    /// Container used for passing alert messages to the UI
    /// </summary>
    public class UIAlert
    {
        //Defines the alert types saved to the ViewData container
        public enum AlertType
        {
            Success,
            Error,
            Warning,
            Info,
            Danger
        }

        /// <summary>
        /// The enum AlertType of the message
        /// </summary>
        public string Type      { get; set; }

        /// <summary>
        /// The message to show in the alert
        /// </summary>
        public string Message   { get; set; }


        public UIAlert(AlertType type, string message)
        {
            if(type == AlertType.Error)
            {
                //The bootstrap alert class used to denote an error, is alert-danger
                //  Thus, we must convert error, to danger
                Type = AlertType.Danger.ToString().ToLower();
            }
            else
            {
                Type = type.ToString().ToLower();
            }
            Message = message;
        }
    }
}