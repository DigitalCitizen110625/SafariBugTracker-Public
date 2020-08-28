namespace SafariBugTracker.WebApp.Models.Settings
{

    /// <summary>
    /// Contains the key used to allow the web application to access database resources through the issue api
    /// </summary>
    public class IssueApiKey
    {
        /// <summary>
        /// Authorization key for accessing the api's
        /// </summary>
        public string ApiKey { get; set; }
    }
}
