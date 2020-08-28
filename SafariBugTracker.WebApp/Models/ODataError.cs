using Newtonsoft.Json;
using System.Collections.Generic;

namespace SafariBugTracker.WebApp.Models
{
    /// <summary>
    /// Defines the properties of an error return from the Azure Table Storage rest api
    /// </summary>
    /// <remarks>
    /// SOURCE: https://stackoverflow.com/questions/32943935/deserializing-odata-error-messages
    /// </remarks>
    public class ODataError
    {
        [JsonProperty("odata.error")]
        public ODataErrorCodeMessage Error { get; set; }
    }

    /// <summary>
    /// Contains the entirety of the OData error
    /// </summary>
    public class ODataErrorCodeMessage
    {
        /// <summary>
        /// String code indicating the error (e.g. Bad_Request)
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Contains additional meta data on the error, and the error cause description
        /// </summary>
        public ODataErrorMessage Message { get; set; }

        /// <summary>
        /// Collection containing additional information on the error including the error items, and their values
        /// </summary>
        public List<ExtendedErrorValue> Values { get; set; }
    }

    /// <summary>
    /// Provides additional information on the error including the error items, and their values
    /// </summary>
    public class ExtendedErrorValue
    {
        public string Item { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Provides additional information on the error message
    /// </summary>
    public class ODataErrorMessage
    {
        /// <summary>
        /// Language code of the error message (e.g. "en" for English)
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// The actual message/description of what went wrong (e.g. Invalid value specified for...)
        /// </summary>
        public string Value { get; set; }
    }
}