using Newtonsoft.Json;
using System.Collections.Generic;

namespace SafariBugTracker.WebApp.Models
{
    /// <summary>
    /// Used to contain the results of a query against a table stored on Azure. Matches the 
    /// ODate Protocol notation used by the Azure storage api.
    /// </summary>
    public class ODataNote
    {
        /* Example: 
         *     "odata.metadata": "https://safaribugtracker.table.core.windows.net/$metadata#SafariUserNotes",
         *       "value": [
         *       {
         *           "odata.etag": "W/\"datetime'2020-06-11T17%3A25%3A03.6340735Z'\"",
         *           "PartitionKey": "QueryPartitionKeyTest",
         *           "RowKey": "a042e591-c37c-496a-a966-a481aa847879",
         *           "Timestamp": "2020-06-11T17:25:03.6340735Z",
         *           "Title": "Query Test Title",
         *           "Content": "This is a test of the query method"
         *       }
         *   ]
         */
        [JsonProperty("odata.metadata")]
        public string MetaData { get; set; }

        [JsonProperty("value")]
        public List<Note> Value { get; set; }
    }
}