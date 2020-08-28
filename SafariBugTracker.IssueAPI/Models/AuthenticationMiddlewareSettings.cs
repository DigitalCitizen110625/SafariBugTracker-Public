namespace SafariBugTracker.IssueAPI.Models
{

    /// <summary>
    /// Defines the customizable properties of the custom authentication middleware class
    /// </summary>
    public class AuthenticationMiddlewareSettings
    {

        /// <summary>
        /// Contains the alpha numeric key that must be matched in the incoming request
        /// headers, in order to access the api
        /// </summary>
        public string AuthKey { get; set; }
    }
}