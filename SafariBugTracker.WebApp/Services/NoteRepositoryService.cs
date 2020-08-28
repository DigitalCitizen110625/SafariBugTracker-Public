using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SafariBugTracker.WebApp.Models;
using SafariBugTracker.WebApp.Models.Settings;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SafariBugTracker.WebApp.Services
{
    /// <summary>
    /// Singleton service that acts as a middle man between the app, and the Azure Table Storage api. 
    /// Uses an HttpClient, and OData protocol to perform CRUD operations.
    /// </summary>
    /// <remarks>
    /// For a list of available REST API commands, please see: 
    ///     https://docs.microsoft.com/en-us/rest/api/storageservices/table-service-rest-api
    /// </remarks>
    public class NoteRepositoryService : HttpService
    {
        #region Properties, Fields, Constructors


        /// <summary>
        /// Base class for sending http requests and receiving responses
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// base portion of the uri without the table name, query string, and SAS token
        /// </summary>
        private readonly string _baseUri;

        /// <summary>
        /// SAS token string needed to authenticate the user and application accessing the Azure storage account
        /// </summary>
        private readonly string _sasToken;

        /// <summary>
        /// Name of the target table in the storage account
        /// </summary>
        private readonly string _tableName;

        /// <summary>
        /// Default time before the request is timed out (30 sec)
        /// </summary>
        private readonly TimeSpan _defaultTimeOut = new TimeSpan(0, 0, 30);

        /// <summary>
        /// As per Microsoft documentation: JSON is the recommended payload format, and is the only format supported 
        /// for versions 2015-12-11 and later
        /// </summary>
        private const string _contentType = "application/json";


        public NoteRepositoryService(IOptions<AzureTableSettings> settings)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = _defaultTimeOut;

            _baseUri = $"https://{settings.Value.AccountName}.table.core.windows.net/";
            _sasToken = settings.Value.SasToken;
            _tableName = settings.Value.TableName;

            _httpClient.BaseAddress = new Uri(_baseUri);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_contentType));
        }



        #endregion
        #region PrivateMethods



        /// <summary>
        /// Checks the partiionKey, and rowKey properties to ensure they aren't null, or empty
        /// </summary>
        /// <param name="record">Record used for the check operation</param>
        /// <exception cref="">Throws ArgumentNullException on the first detected null property</exception>
        private void EnsureKeysAreValid(IAzureTableRecord record)
        {
            if (string.IsNullOrEmpty(record.PartitionKey))
            {
                throw new ArgumentNullException($"{nameof(record.PartitionKey)} cannot be null");
            }
            if (string.IsNullOrEmpty(record.RowKey))
            {
                throw new ArgumentNullException($"{nameof(record.RowKey)} cannot be null");
            }
        }


        /// <summary>
        /// Builds the query string when filtering results from a table.
        /// Current version only supports the $filter command and does not allow the use of 
        /// $select, or $top
        /// </summary>
        /// <param name="searchParams">Key value pairs of the property to filter and the value</param>
        /// <returns>Complete query string (e.g. $filter=Title%20eq%20'Test Title')</returns>
        private string BuildSearchUri(Dictionary<string, string> searchParams)
        {
            //Query string is: Base uri + ()?$filter={PROPERTY}%20eq%20'{VALUE}'%20and%20{PROPERTY2}%20eq%20'{VALUE}'
            //The property name, operator, and constant value must be separated by URL-encoded spaces (i.e. %20)
            const string baseSearchUri = "&$filter=";
            const string baseParameterString = "{PROPERTY}%20eq%20'{VALUE}'";
            const string propertyPlaceHolder = "{PROPERTY}";
            const string valuePlaceHolder = "{VALUE}";
            const string andKeyword = "%20and%20";
            StringBuilder searchUri = new StringBuilder(baseSearchUri);

            var lastItem = searchParams.Keys.Count;
            int index = 0;

            //Add the key-value pairs to the search string one at a time
            foreach (var item in searchParams)
            {
                searchUri.Append(baseParameterString);
                searchUri.Replace(propertyPlaceHolder, item.Key);
                searchUri.Replace(valuePlaceHolder, item.Value);

                //If theres another parameter following this one, then connect the two
                //with the andKeyword
                if (index < lastItem - 1)
                {
                    searchUri.Append(andKeyword);
                }
                index++;
            }

            return searchUri.ToString();
        }


        /// <summary>
        /// Extracts the message contents based on the status code of the argument
        /// </summary>
        /// <param name="response">HttpResponseMessage containing the http status code of the request, and the serialized response message</param>
        /// <returns> Null if the status code indicates success, or a string indicating the error message </returns>
        private async Task<string> ExtractResponseMessage(HttpResponseMessage response)
        {
            if (!WasRequestSuccessful(response))
            {
                var errorMessage = await DeserializeError(response);
                return FormatErrorMessage(errorMessage);
            }

            //Under the OData protocol, upon successful completion of an insertion, update, or delete request, HttpStatus 200, or 204 is returned.
            //Neither status provides a useful message so we shall return null
            return null;
        }


        /// <summary>
        /// Checks if the request has a success http status code, or error 
        /// </summary>
        /// <param name="response">HttpResponseMessage to check if ti contains the status code or error</param>
        /// <returns>True if the response had status code 2XX, false otherwise</returns>
        private bool WasRequestSuccessful(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Converts the HttpResponseMessage to a string and deserializes it into an OData error object
        /// </summary>
        /// <param name="response">HttpResponseMessage containing the error message</param>
        /// <returns>Task of the deserialzied error as a string</returns>
        private async Task<string> DeserializeError(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var deserializedMessage = JsonConvert.DeserializeObject<ODataError>(responseString);
            return deserializedMessage.Error.Message.Value;
        }


        /// <summary>
        /// Cuts off the meta data from the error message, and removes all non-white space and letter chars
        /// </summary>
        /// <param name="baseErrorMessage">Error message with the extra meta data</param>
        /// <returns>String with the error message converted to plain English</returns>
        private string FormatErrorMessage(string baseErrorMessage)
        {
            //Regex clean all non letter chars, and whitespace from the response, and truncate the string after the new line char
            var cleanedString = baseErrorMessage.Substring(0, baseErrorMessage.IndexOf("\n"));
            Regex regex = new Regex("[^A-Za-z ]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return regex.Replace(cleanedString, String.Empty); ;
        }


        /// <summary>
        /// Converts the HttpResponseMessage to a string, and deserializes it to a container with a collection of notes
        /// </summary>
        /// <param name="response">HttpResponseMessage containing the </param>
        /// <returns>Task of deserialized notes held in a ONote container</returns>
        private async Task<ODataNote> ExtractNoteList(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ODataNote>(responseString);
        }



        #endregion
        #region PublicMethods



        /// <summary>
        /// Builds the search query, and sends the request to the Azure Storage Service rest api
        /// </summary>
        /// <param name="searchParams">Key-value pairs of the properties, to filter, and their values</param>
        /// <exception cref=""> Throws ArgumentException, and an exception resulting from HttpService.QueryAsync</exception>
        /// <returns>List of note objects that matched the query, or null on error</returns>
        public async Task<(List<Note> Notes, string error)> QueryTableAsync(Dictionary<string, string> searchParams)
        {
            if (searchParams is null || searchParams.Count < 1)
            {
                throw new ArgumentException("Error: Search parameters cannot be null");
            }
            var searchUri = BuildSearchUri(searchParams);
            searchUri = _tableName + _sasToken + searchUri;
            var response = await base.QueryAsync(_httpClient, searchUri, _contentType);
            var responseMessage = await ExtractResponseMessage(response);


            //Under the OData protocol, upon successful completion of an insertion, update, or delete request, an HttpStatus 200, or 204 is returned.
            //Neither status provides a useful message so null is the expected return from the extraction process. 
            //If an error occurred, then we'd expect a string with the error message
            if (responseMessage != null)
            {
                //Encountered error during query, so no records were found
                return (null, responseMessage);
            }
            else
            {
                //Response was as successful 2xx
                var ONoteCollection = await ExtractNoteList(response);
                return (ONoteCollection.Value, null);
            }
        }


        /// <summary>
        /// Builds the resource uri, and sends a insert/POST http request to the Azure Storage Service rest api
        /// </summary>
        /// <typeparam name="T">Type of the resource to serialize and send in the request</typeparam>
        /// <param name="note">Note object to serialize</param>
        /// <returns>String indicating the result of the operation</returns>
        public async Task<string> InsertNoteAsync<T>(T note) where T: Note, new()
        {
            EnsureKeysAreValid(note);
            var searchUri = _tableName + _sasToken;

            try
            {
                var response = await base.InsertAsync<Note>(note, _httpClient, searchUri, _contentType);
                var responseString = await ExtractResponseMessage(response);
                if(responseString != null)
                {
                    //We've extracted an error message from the table service api
                    return responseString;
                }
                else
                {
                    //Response was as successful 2xx
                    return "Note Saved!";
                }
            }
            catch (ArgumentNullException e)
            {
                Log.Error(e, e.Message);
                return $"Internal Error: One of the notes fields was invalid, please report the error for assistance";
            }
            catch (SocketException e)
            {
                Log.Error(e, e.Message);
                return "Connection Error: Remote Storage Service is unresponsive, please try again later";
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return "Unknown Error: Please try again later";
            }
        }


        /// <summary>
        /// Builds the resource uri, and sends a update/PUT http request to the Azure Storage Service rest api
        /// </summary>
        /// <typeparam name="T">Type of the resource to serialize and send in the request</typeparam>
        /// <param name="note">Note object to serialize</param>
        /// <returns>String indicating the result of the operation</returns>
        public async Task<string> UpdateNoteAsync<T>(T note) where T : Note, new()
        {
            EnsureKeysAreValid(note);
            var searchUri = $"{_tableName}(PartitionKey='{note.PartitionKey}',RowKey='{note.RowKey}')" + _sasToken;

            try
            {
                var response = await base.UpdateAsync<Note>(note, _httpClient, searchUri, _contentType);
                var responseString = await ExtractResponseMessage(response);
                if (responseString != null)
                {
                    //We've extracted an error message from the table service api
                    return responseString;
                }
                else
                {
                    //Response was as successful 2xx
                    return "Note Updated!";
                }
            }
            catch (ArgumentNullException e)
            {
                Log.Error(e, e.Message);
                return $"Internal Error: One of the notes fields was invalid, please report the error for assistance";
            }
            catch (SocketException e)
            {
                Log.Error(e, e.Message);
                return "Connection Error: Remote Storage Service is unresponsive, please try again later";
            }
			catch (Exception e)
            {
                Log.Error(e, e.Message);
                return "Unknown Error: Please try again later";
            }
        }


        /// <summary>
        /// Builds the resource uri, and sends a delete http request to the Azure Storage Service rest api
        /// </summary>
        /// <typeparam name="T">Type of the resource to delete</typeparam>
        /// <param name="note">Object containing the partition key, and row key of the target record to delete</param>
        /// <returns>String indicating the result of the operation</returns>
        public async Task<string> DeleteNoteAsync<T>(T note) where T : Note, new()
        {
            EnsureKeysAreValid(note);
            var searchUri = $"{_tableName}(PartitionKey='{note.PartitionKey}',RowKey='{note.RowKey}')" + _sasToken;

            try
            {
                var response = await base.DeleteAsync(_httpClient, searchUri, _contentType);
                var responseString = await ExtractResponseMessage(response);
                if(responseString is null)
                {
                    responseString = "Delete Successful";
                }
                return responseString;
            }
            catch (ArgumentNullException e)
            {
                Log.Error(e, e.Message);
                return $"Internal Error: One of the notes fields was invalid, please report the error for assistance";
            }
            catch (SocketException e)
            {
                Log.Error(e, e.Message);
                return "Connection Error: Remote Storage Service is unresponsive, please try again later";
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return "Unknown Error: Please try again later";
            }
        }


        #endregion
    }//class
}//namespace