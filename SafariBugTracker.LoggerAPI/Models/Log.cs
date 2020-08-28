using System;
using System.ComponentModel.DataAnnotations;

namespace SafariBugTracker.LoggerAPI.Models
{

    public interface ILog
    { 
        public string Id        { get; set; }
        public string Source    { get; set; }
        public string Date      { get; set; }
        public string Severity  { get; set; }
        public string Message   { get; set; }
    }


    /// <summary>
    /// Defines the structure and contents of logs submitted to the logger api
    /// </summary>
    [Serializable]
    public class Log : ILog
    {
        /// <summary>
        /// Id of the specific log. Used for finding a specific log.
        /// Must be a GUID string of exactly 36 chars in length
        /// </summary>
        [StringLength(36)]
        public string Id { get; set; }

        /// <summary>
        /// Source (i.e. the name) of the application, or executable binary that sent the log
        /// </summary>
        [Required]
        public string Source    { get; set; }

        /// <summary>
        /// Time-stamp when the log was created. Dates will be in the form of dd-MM-yyyy
        /// </summary>
        public string Date      { get; set; }

        /// <summary>
        /// Severity of the log, defined in SafariBugTracker.LoggerAPI.Services.LogSeverity
        /// </summary>
        [Required]
        public string Severity  { get; set; }

        /// <summary>
        /// Message content of the log
        /// </summary>
        [Required]
        public string Message   { get; set; }
    }
}