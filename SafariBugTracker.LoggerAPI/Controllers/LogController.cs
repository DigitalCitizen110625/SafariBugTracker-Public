using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SafariBugTracker.LoggerAPI.Models;
using SafariBugTracker.LoggerAPI.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace LoggerAPI.Controllers
{
    /// <summary>
    /// Primary controller for executing all CRUD operations related to incoming logs
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        #region Properties


        /// <summary>
        /// Contains the config values from the appsettings.json file
        /// </summary>
        private readonly AuthorizationSettings _authSettings;

        /// <summary>
        /// Service used to actually perform all CRUD operations for the incoming logs
        /// </summary>
        private readonly ILoggerService _logService;

        public LogController(IOptions<AuthorizationSettings> authSettings, ILoggerService logger)
        {
            _authSettings = authSettings.Value ?? throw new ArgumentNullException(nameof(IOptions<AuthorizationSettings>)); ;
            _logService = logger ?? throw new ArgumentNullException(nameof(ILoggerService));
        }


        #endregion
        #region PublicMethods


        /// <summary>
        /// Gets all logs from the primary storage location, as stated in the appsettings
        /// </summary>
        /// <remarks>Route: api/log/</remarks>
        /// <returns>Collection of all log objects</returns>
        [HttpGet]
        [HttpHead]
        public ActionResult<Log> GetLogs()
        {
            var logs = _logService.Get();
            return Ok(logs);
        }

        /// <summary>
        /// Gets a single log with the matching GUID
        /// </summary>
        /// <remarks>Route: api/log/logID</remarks>
        /// <param name="logID">The 36 char alpha-numeric string used to identify a specific log </param>
        /// <returns></returns>
        [HttpGet("{logID}")]
        [HttpHead]
        public ActionResult<Log> GetLogs(Guid logID)
        {
            if(logID == null)
            {
                return BadRequest("Log Id cannot be null");
            }

            var log = _logService.Get(logID);
            if(log == null)
            {
                return NotFound($"Resource with ID: {logID} does not exist");
            }
            return Ok(log);
        }

        /// <summary>
        /// Saves the incoming logs according to the config options
        /// </summary>
        /// <remarks>Route: api/log/submit/authcode </remarks>
        /// <param name="log"> Log details from the incoming http message body </param>
        /// <returns> ObjectResult containing the HTTP status code and result message </returns>
        [HttpPost]
        [Route("submit")]
        public IActionResult Submit(string authcode, dynamic dynamicLog)
        {
            if (dynamicLog is null)
            {
                return BadRequest("Ensure all parameters are supplied and non-null");
            }

            //Confirm the request was from a legitimate source
            if (!AuthorizedAccess(authcode))
            {
                return Unauthorized("Authorization Code Invalid");
            }

            
            try
            {
                //The incoming logs are set to dynamic, we will convert them to a string, and extract the properties, and their values
                var serializedLog = JsonSerializer.Serialize(dynamicLog);


                /* Example serialized log: "{\"events\":[{\"Timestamp\":\"2020-08-23T19:20:35.7353541-04:00\",\"Level\":\"Warning\",\"MessageTemplate\":\"This is a test message of the serilog feature in safaribugtracker\",\"RenderedMessage\":\"This is a test message of the serilog feature in safaribugtracker\",\"Properties\":{\"Application\":\"SafariWebApp\"}}]}"
                 * Pattern: \\"([A-Za-z]+)\\"([:])\\"(.+?)\\"
                 * Result: 
                 *  • Group 1 = Timestamp
                 *  • Group 2 = : (i.e. the colon char)
                 *  • Group 3 = 2020-08-23T19:20:35.7353541-04:00
                 *  
                 *  • Group 1 = Level
                 *  • Group 2 = : (i.e. the colon char)
                 *  • Group 3 = Warning
                 *  
                 *  • Group 1 = MessageTemplate
                 *  • Group 2 = : (i.e. the colon char)
                 *  • Group 3 = This is a test message of the serilog feature in safaribugtracker
                 *  
                 *  • Group 1 = RenderedMessage
                 *  • Group 2 = : (i.e. the colon char)
                 *  • Group 3 = This is a test message of the serilog feature in safaribugtracker
                 *  
                 *  • Group 1 = Application
                 *  • Group 2 = : (i.e. the colon char)
                 *  • Group 3 = SafariWebApp
                 *  And so on...
                 */
                const string pattern = "\\\"([A-Za-z]+)\\\"([:])\\\"(.+?)\\\"";
                Regex rx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection matches = rx.Matches(serializedLog);


                /* As per the comment above, each match will have the following groups:
                 *  • Group 0 = The complete matched string 
                 *  • Group 1 = Property
                 *  • Group 2 = : 
                 *  • Group 3 = PropertyValue
                 */
                const int propertyNameGroupIndex = 1;
                const int propertyValueGroupIndex = 3;
                string propertyName = string.Empty;
                string propertyValue = string.Empty;

                var logEvent = new Dictionary<string, object>();
                for (int i = 0; i< matches.Count; i++)
                {
                    propertyName = matches[i].Groups[propertyNameGroupIndex].Value;
                    propertyValue = matches[i].Groups[propertyValueGroupIndex].Value;
                    
                    if(propertyName == "Timestamp" && i > 0)
                    {
                        _logService.Save(new DynamicDictionary(logEvent));
                        logEvent.Clear();
                    }

                    logEvent.Add((string)propertyName.Clone(), propertyValue.Clone());                    

                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok("Submit Successful");
        }


        #endregion
        #region PrivateMethods


        /// <summary>
        /// Checks if the logs authorization code matches the accepted list of codes
        /// </summary>
        /// <param name="authCode"> Authorization code packaged with the log details </param>
        /// <returns> True if the auth code was correct </returns>
        private bool AuthorizedAccess (string authCode)
        {
            if(string.Equals(authCode, _authSettings.AuthCode))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        #endregion
    }//class
}//namespace