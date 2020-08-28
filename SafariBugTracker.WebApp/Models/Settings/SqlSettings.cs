namespace SafariBugTracker.WebApp.Models.Settings
{
    /// <summary>
    /// Contains the connection string for the sql server used for the identity service
    /// </summary>
    public class SqlSettings
    {
        /// <summary>
        /// Connection string to the SQL database where the EFC app context data is accessible
        /// </summary>
        public string SafariBugTrackerWebAppContextConnection { get; set; }
    }
}