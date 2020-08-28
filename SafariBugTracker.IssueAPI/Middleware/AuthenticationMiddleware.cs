using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SafariBugTracker.IssueAPI.Models;
using System.Threading.Tasks;

namespace SafariBugTracker.IssueAPI.Middleware
{
    /// <summary>
    /// Basic authorization middleware component that checks the request header and ensures it contains the 
    /// correct api key in order to access the api's resources
    /// </summary>
    /// <remarks>
    /// SOURCE: https://stackoverflow.com/questions/38977088/asp-net-core-web-api-authentication
    /// </remarks>
    public class AuthenticationMiddleware
    {
        /// <summary>
        /// Allows the class to access the next component in the pipeline once finished
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Allows the class to access the authorization options found in appsettings.json 
        /// </summary>
        private readonly IOptions<AuthenticationMiddlewareSettings> _options;

        public AuthenticationMiddleware(RequestDelegate next, IOptions<AuthenticationMiddlewareSettings> options)
        {
            _next = next;
            _options = options;
        }

        /// <summary>
        /// Checks the authentication header to ensure it contains the correct api key. All requests 
        /// not containing the authorization key will be rejected immediately 
        /// </summary>
        /// <param name="context"> HttpContext containing the details of the request</param>
        /// <returns> Empty task, or http status code 401 indicating the request was rejected</returns>
        public async Task Invoke(HttpContext context)
        {
            //Get the authorization field from the header and ensure it's filled in
            string authHeader = context.Request.Headers["Authorization"];
            if(string.IsNullOrEmpty (authHeader) || !authHeader.Equals(_options.Value.AuthKey))
            {
                //No authorization header
                context.Response.StatusCode = 401; //Unauthorized
                return;
            }
            else
            {
                //Api key was correct so lets move on to the next middleware component
                await _next.Invoke(context);
            }


            //Alternative approach using a username and password authentication
            #region UsernameAndPasswordAuth

            //string encodedUsernamePassword = authHeader.Substring("Account-".Length).Trim();
            //Encoding encoding = Encoding.GetEncoding("iso-8859-1");
            //string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

            //int seperatorIndex = usernamePassword.IndexOf(':');

            //var username = usernamePassword.Substring(0, seperatorIndex);
            //var password = usernamePassword.Substring(seperatorIndex + 1);

            //if (username == "test" && password == "test")
            //{
            //    await _next.Invoke(context);
            //}
            //else
            //{
            //    context.Response.StatusCode = 401; //Unauthorized
            //    return;
            //}

            #endregion
        }

    }//class
}//namespace