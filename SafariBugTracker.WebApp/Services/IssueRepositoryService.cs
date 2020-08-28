using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SafariBugTracker.WebApp.Models;
using SafariBugTracker.WebApp.Models.Settings;
using SafariBugTracker.WebApp.Models.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using Log = Serilog.Log;

namespace SafariBugTracker.WebApp.Services
{
    public interface IIssueRepositoryService
    {
        public Task<(IIssue issue, string error)> GetIssue(string id);
        public Task<(IEnumerable<IIssue> issues, string error)> QueryIssues(string queryString);
        public Task<(bool success, string message)> InsertIssue<T>(T issue) where T : IIssue, new();
        public Task<(bool success, string message)> UpdateIssue<T>(T issue) where T : IIssue, new();
        public Task<(bool success, string message)> DeleteIssue(string id);
        public Task<(DashboardKPIViewModel dasboardKPI, string error)> GetProjectMetrics(string team);
    }

    /// <summary>
    /// Singleton service that acts as a middle man between this app, and the issueAPI, which is responsible for performing all CRUD 
    /// operations on the submitted issues. 
    /// </summary>
    public class IssueRepositoryService : HttpService, IIssueRepositoryService
    {
        #region Properties, Fields, Constructor


        /// <summary>
        /// Provides a means of sending HTTP requests and receiving HTTP responses from a resource identified by a URI.
        /// As per the MSDN documentation, HttpClient is intended to be instantiated once per application, rather than per-use. 
        /// See https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=netcore-3.1
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Defines the content type header property of the request
        /// </summary>
        private const string _contentType = "application/json";

        /// <summary>
        /// When concatenated with the httpClient.BaseAddress, it gives the url path to the issue CRUD operations endpoint at the api
        /// </summary>
        private const string _apiIssuePath = "issues/";

        /// <summary>
        /// When concatenated with the httpClient.BaseAddress, it gives the url path to the issue metrics endpoint at the api
        /// </summary>
        private const string _apiMetricsPath = "metrics/";

        public IssueRepositoryService(IOptions<IssueRepositorySettings> issueSettings, IOptions<IssueApiKey> issueApiKey)
        {
            EnsureSettingsAreInitialized(issueSettings.Value, issueApiKey.Value);

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(issueSettings.Value.BaseUri);
            _httpClient.Timeout = new TimeSpan(0, 0, 10);
            _httpClient.DefaultRequestHeaders.Clear();

            //We only want a response in JSON format 
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_contentType));

            //Add the api key to the authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(issueApiKey.Value.ApiKey);
        }



        #endregion
        #region PrivateMethods



        /// <summary>
        /// Checks to ensure that all required settings values have been provided
        /// </summary>
        /// <remarks>Throws ArgumentNullException on any null values </remarks>
        /// <param name="issueSettings">IssueRepositorySettings containing the config values from the appsettings file</param>
        /// <param name="issueApiKey">IssueApiKey containing the config values from the appsettings file </param>
        private static void EnsureSettingsAreInitialized(IssueRepositorySettings issueSettings, IssueApiKey issueApiKey)
        {
            try
            {
                if (issueSettings.BaseUri == null)
                {
                    throw new ArgumentNullException($"{nameof(issueSettings.BaseUri)}: cannot be null");
                }
                else if (issueApiKey.ApiKey == null)
                {
                    throw new ArgumentNullException($"{nameof(issueApiKey.ApiKey)}: cannot be null");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                throw;
            }
        }


        /// <summary>
        /// Checks if the argument is null, empty, or filled with just white space
        /// </summary>
        /// <param name="id">Issue id string to check</param>
        /// <remarks>Throws ArgumentNullException on null entry </remarks>
        private void EnsureIdValid(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException($"{nameof(id)} cannot be null");
            }
        }


        /// <summary>
        /// Deserializes the contents of the http response message, to the type specified in the parameters
        /// </summary>
        /// <param name="response">HttpResponseMessage containing the contents of the response</param>
        /// <returns>Task of the deserialzied response</returns>
        private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseString);
        }


        /// <summary>
        /// Returns an error message based on the status and contents of the request
        /// </summary>
        /// <param name="response">HttpResponseMessage containing the status code and contents of the message </param>
        /// <returns>User friendly string indicating the cause of the error </returns>
        private async Task<string> ExtractErrorMessage(HttpResponseMessage response)
        {
            string error = string.Empty;
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.NotFound:
                case System.Net.HttpStatusCode.BadRequest:
                case System.Net.HttpStatusCode.Unauthorized:
                    error = await DeserializeResponse<string>(response);
                    if (error != null)
                    {
                        return error;
                    }
                    return response.ReasonPhrase;


                case System.Net.HttpStatusCode.ServiceUnavailable:
                    return "The service is currently unavailable, please try again later";


                case System.Net.HttpStatusCode.InternalServerError:
                    try
                    {
                        //The API has experienced an internal error
                        error = await DeserializeResponse<string>(response);
                        Log.Error($"{error}");
                    }
                    catch (Exception e)
                    {
                        error = await response.Content.ReadAsStringAsync();
                        Log.Error(e, e.Message);
                    }
                    return "The remote service experienced an internal error, please try again later";



                default:
                    try
                    {
                        //If the de-serialization process fails, then the error is not in the correct JSON format. 
                        //Likely the cause is an AggregateException from the api
                        error = await DeserializeResponse<string>(response);
                    }
                    catch (Exception e)
                    {
                        error = await response.Content.ReadAsStringAsync();
                        Log.Error(e, e.Message);
                    }
                    return "The remote service experienced an unknown error, please try again later";
            }
        }



        #endregion
        #region PublicMethods


        /// <summary>
        /// Sends a GET request to the issue api, for a collection of issues that match the queryString
        /// </summary>
        /// <typeparam name="T">Type of the records to get </typeparam>
        /// <param name="queryString">String detailing the query parameters </param>
        /// <returns>Collection of issue type objects matching the query, null otherwise </returns>
        public async Task<(IEnumerable<IIssue> issues, string error)> QueryIssues(string queryString)
        {
            try
            {
                string queryUri = _apiIssuePath + "search" + queryString;             
                var response = await base.QueryAsync(_httpClient, queryUri, _contentType);
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await ExtractErrorMessage(response);
                    return (null, errorMessage);
                }

                var issues = await DeserializeResponse<IEnumerable<Issue>>(response);
                return (issues, null);
            }
            catch (SocketException e)
            {
                Log.Error(e, e.Message);
                return (issues: null, error: "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch(HttpRequestException e)
            {
                Log.Error(e, e.Message);
                return (issues: null, error: "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return (issues: null, error: "Unknown Error: Please try again later");
            }
        }


        /// <summary>
        /// Identical to a query action, except that it requests a single record with a matching id field
        /// </summary>
        /// <typeparam name="T">Type of the record to get </typeparam>
        /// <param name="id">String if of the record to get from the api </param>
        /// <returns>IIssue with the matching id, null otherwise <returns>
        public async Task<(IIssue issue, string error)> GetIssue(string id)
        {
            try
            {
                EnsureIdValid(id);
                string endpoint = _apiIssuePath + id;
                var response = await base.QueryAsync(_httpClient, endpoint, _contentType);
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await ExtractErrorMessage(response);
                    return (null, errorMessage);
                }

                var issues = await DeserializeResponse<Issue>(response);
                return (issues, null);
            }
            catch (ArgumentNullException e)
            {
                Log.Error(e, e.Message);
                return (issue: null, error: "Internal Error: One of the notes fields was invalid, please report the error for assistance");
            }
            catch (SocketException e)
            {
                Log.Error(e, e.Message);
                return (issue: null, error: "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch (HttpRequestException e)
            {
                Log.Error(e, e.Message);
                return (issue: null, error: "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return (issue: null, error: "Unknown Error: Please try again later");
            }
        }


        /// <summary>
        /// Sends a POST request to the issue api, to insert the new resource
        /// </summary>
        /// <typeparam name="T">Type of record to insert </typeparam>
        /// <param name="issue">Object containing the details of the new record </param>
        /// <returns>String indicating the result of the operation</returns>
        public async Task<(bool success, string message)> InsertIssue<T>(T issue) where T : IIssue, new()
        {
            try
            {
                var response = await base.InsertAsync<T>(issue, _httpClient, _apiIssuePath, _contentType);
                if (!response.IsSuccessStatusCode)
                {
                    return (false, await ExtractErrorMessage(response));
                }
                return (true, "Issue Submitted Successfully");
            }
            catch (ArgumentNullException e)
            {
                Log.Error(e, e.Message);
                return (false, $"Internal Error: One of the notes fields was invalid, please report the error for assistance");
            }
            catch (SocketException e)
            {
                Log.Error(e, e.Message);
                return (false, "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch (HttpRequestException e)
            {
                Log.Error(e, e.Message);
                return (false, "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return (false, "Unknown Error: Please try again later");
            }
        }


        /// <summary>
        /// Sends a PUT request to the issue api, to update a record with the matching id
        /// </summary>
        /// <typeparam name="T">Type of record to update </typeparam>
        /// <param name="issue">Object containing the details of the record to update </param>
        /// <returns>String indicating the result of the operation</returns>
        public async Task<(bool success, string message)> UpdateIssue<T>(T issue) where T : IIssue, new()
        {
            try
            {
                EnsureIdValid(issue.Id);
                string endpoint = _apiIssuePath + issue.Id;
                var response = await base.UpdateAsync<T>(issue, _httpClient, endpoint, _contentType);
                if (!response.IsSuccessStatusCode)
                {
                    return (false, await ExtractErrorMessage(response));
                }
                return (true, "Update Successful");
            }
            catch (ArgumentNullException e)
            {
                Log.Error(e, e.Message);
                return (false, $"Internal Error: One of the notes fields was invalid, please report the error for assistance");
            }
            catch (SocketException e)
            {
                Log.Error(e, e.Message);
                return (false, "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch (HttpRequestException e)
            {
                Log.Error(e, e.Message);
                return (false, "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return (false, "Unknown Error: Please try again later");
            }
        }


        /// <summary>
        /// Sends a DELETE request to the issue api, for the record with a matching id
        /// </summary>
        /// <param name="id">Object id of the record to delete </param>
        /// <returns>String indicating the result of the operation</returns>
        public async Task<(bool success, string message)> DeleteIssue(string id)
        {
            try
            {
                EnsureIdValid(id);
                string endpoint = _apiIssuePath + id;
                var response = await base.DeleteAsync(_httpClient, endpoint, _contentType);
                if (!response.IsSuccessStatusCode)
                {
                    return (false, await ExtractErrorMessage(response));
                }
                return (true, "Delete Successful");
            }
            catch (ArgumentNullException e)
            {
                Log.Error(e, e.Message);
                return (false, $"Internal Error: One of the notes fields was invalid, please report the error for assistance");
            }
            catch (SocketException e)
            {
                Log.Error(e, e.Message);
                return (false, "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch (HttpRequestException e)
            {
                Log.Error(e, e.Message);
                return (false, "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return (false, "Unknown Error: Please try again later");
            }
        }

        /// <summary>
        /// Sends a GET request to the API for a collection of metrics printable on the dashboard view
        /// </summary>
        /// <param name="project">The project to query for when gathering the metrics</param>
        /// <returns>Tuple of (DashboardKPIViewModel, null), on success, and (mull, string) if an error was detected</returns>
        public async Task<(DashboardKPIViewModel dasboardKPI, string error)> GetProjectMetrics(string project)
        {
            try
            {
                string targetUri = _apiMetricsPath + project;
                var response = await base.QueryAsync(_httpClient, targetUri, _contentType);

                if (!response.IsSuccessStatusCode)
                {
                    return (null, await ExtractErrorMessage(response));
                }

                return (await DeserializeResponse<DashboardKPIViewModel>(response), null);
            }

            catch (ArgumentNullException e)
            {
                Log.Error(e, e.Message);
                return (null, "Internal Error: One of the notes fields was invalid, please report the error for assistance");
            }
            catch (SocketException e)
            {
                Log.Error(e, e.Message);
                return (null, "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch (HttpRequestException e)
            {
                Log.Error(e, e.Message);
                return (null, "Connection Error: Remote api is unresponsive, please try again later");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);               
                return (null, "Unknown Error: Please try again later");

            }
        }



        #endregion
        #region GetResourcesThroughHttpRequestMessage



        /// <summary>
        /// Functionally identical to the GetResource methods above, only that it creates the Http request Message headers manually,
        /// and sends the request with _httpClient.SendAsync(request), instead of using _httpClient.GetAsync() which is the shortcut.
        /// GetAsync() automatically builds the HttpRequestMessage as a HttpMethod.Get request, and lets you skip the first two steps
        /// shown below
        /// </summary>
        /// <param name="issue">Contains details of the record to request</param>
        /// <returns>The requested issue with the matching ID requested from the api</returns>
        public async Task<Issue> GetResourcesThroughHttpRequestMessage(IIssue issue)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/issues/{issue.Id}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Issue>(content);
        }


        #endregion
    }//class
}//namespace


