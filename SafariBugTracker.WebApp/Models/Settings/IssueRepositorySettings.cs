namespace SafariBugTracker.WebApp.Models.Settings
{
    /// <summary>
    /// Contains the base uri for accessing the issue api
    /// </summary>
    public class IssueRepositorySettings
    {
        /// <summary>
        /// Base uri used for accessing the api responsible for managing the issues/bug records
        /// </summary>
        public string BaseUri { get; set; }
    }
}