using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SafariBugTracker.WebApp.Services
{

    /// <summary>
    /// Base class for all derived http client subclasses. Provides some basic functionality including serialization, 
    /// building/sending an http request, and generic implementations for performing CRUD operations at the specified endpoints.
    /// </summary>
    public class HttpService
    {

        /// <summary>
        /// Serializes the incoming object in either json, or xml, based on the mediaType parameter
        /// </summary>
        /// <typeparam name="T">Type of the argument</typeparam>
        /// <param name="record">Object to be serialized</param>
        /// <param name="mediaType">Http media type used to determine the serialization method. 
        /// Ex: A media type of application/json will serialize the object into json</param>
        /// <exception cref="">Throws ArgumentException if the media type is not application/json , or application/xml</exception>
        /// <returns>Serialized version of the object</returns>
        protected string SerializeObject<T>(T record, string mediaType)
        {
            if (mediaType.IndexOf("json", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return JsonConvert.SerializeObject(record);
            }
            else if(mediaType.IndexOf("xml", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                using (var stringwriter = new System.IO.StringWriter())
                {
                    var serializer = new XmlSerializer(record.GetType());
                    serializer.Serialize(stringwriter, record);
                    return stringwriter.ToString();
                }
            }
            else
            {
                throw new ArgumentException($"Unrecognized media type: {mediaType}");
            }
        }


        /// <summary>
        /// Builds the HttpRequestMessage using the passed in arguments
        /// </summary>
        /// <param name="httpMethod">Type of HttpMethod used to send the request (e.g. HttpMethod.Get)</param>
        /// <param name="requestUri">Target URI of the request</param>
        /// <param name="headerMediaType">Http media type message header </param>
        /// <param name="bodyMediaType">Http media type content header</param>
        /// <param name="bodyContent">Any serialized objects that need be added to the body portion of the message</param>
        /// <exception cref="">
        /// Throws exceptions if an error occurs
        /// </exception>
        /// <returns>
        /// Complete HttpRequestMessage with the headers, and body content filled in
        /// </returns>
        protected HttpRequestMessage BuildRequestMessage(HttpMethod httpMethod, string requestUri, string headerMediaType, string bodyMediaType, string bodyContent = "")
        {
            var request = new HttpRequestMessage(httpMethod, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(headerMediaType));
            request.Content = new StringContent(bodyContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(bodyMediaType);
            return request;
        }


        /// <summary>
        /// Sends an async http request using the passed in HttpClient
        /// </summary>
        /// <param name="httpClient"> HttpClient used to send the request. Must be preconfigured and contain
        /// the base address and default request headers prior to sending the request
        /// </param>
        /// <param name="request">HttpRequestMessage with complete headers, and any serialized data that needs to be sent</param>
        /// <exception cref="">
        /// Throws ArgumentNullException, InvalidOperationException, or HttpRequestException if an error occurs
        /// </exception>
        /// <returns>
        /// Task of type HttpResponseMessage containing the raw response from the endpoint
        /// </returns>
        protected virtual async Task<HttpResponseMessage> SendRequestAsync(HttpClient httpClient, HttpRequestMessage request)
        {
            //Unfortunately, we can't read the response or check if it's a success, since the logger, table service api, and issue api all handle success/errors separately
            return await httpClient.SendAsync(request);
        }


        /// <summary>
        /// Send a GET request to the endpoint
        /// </summary>
        /// <param name="httpClient"> HttpClient used to send the request. Must be preconfigured and contain
        /// the base address and default request headers prior to sending the request
        /// </param>
        /// <param name="requestUri">Target URI of the request</param>
        /// <param name="mediaType">Specification of the http media type (e.g. application/json)</param>
        /// <exception cref="">
        /// Throws exceptions if an error occurs
        /// </exception>
        /// <returns>
        /// Task of type HttpResponseMessage containing the raw response from the endpoint
        /// </returns>
        protected virtual Task<HttpResponseMessage> QueryAsync(HttpClient httpClient, string requestUri, string mediaType)
        {
            var request = BuildRequestMessage(HttpMethod.Get, requestUri, mediaType, mediaType, "");
            return SendRequestAsync(httpClient, request);
        }


        /// <summary>
        /// Insert a new record at the specified endpoint
        /// </summary>
        /// <typeparam name="T"> Type of the record </typeparam>
        /// <param name="record"> Record to serialize and include in the message body </param>
        /// <param name="httpClient"> HttpClient used to send the request. Must be preconfigured and contain
        /// the base address and default request headers
        /// </param>
        /// <param name="requestUri">Target URI of the request</param>
        /// <param name="mediaType">Specification of the http media type (e.g. application/json)</param>
        /// <exception cref="">
        /// Throws exceptions if an error occurs
        /// </exception>
        /// <returns>
        /// Task of type HttpResponseMessage containing the raw response from the endpoint
        /// </returns>
        protected virtual Task<HttpResponseMessage> InsertAsync<T>(T record, HttpClient httpClient , string requestUri, string mediaType)
        {
            var serializedRecord = SerializeObject(record, mediaType);
            var request = BuildRequestMessage(HttpMethod.Post, requestUri, mediaType, mediaType, serializedRecord);
            return SendRequestAsync(httpClient, request);
        }


        /// <summary>
        /// Update the record at the specified endpoint
        /// </summary>
        /// <typeparam name="T">Type of the record</typeparam>
        /// <param name="record">Record to serialize and include in the message body</param>
        /// <param name="httpClient"> HttpClient used to send the request. Must be preconfigured and contain
        /// the base address and default request headers
        /// </param>
        /// <param name="requestUri">Target URI of the request</param>
        /// <param name="mediaType">Specification of the http media type (e.g. application/json)</param>
        /// <exception cref="">
        /// Throws exceptions if an error occurs
        /// </exception>
        /// <returns>
        /// Task of type HttpResponseMessage containing the raw response from the endpoint 
        /// </returns>
        protected virtual Task<HttpResponseMessage> UpdateAsync<T>(T record, HttpClient httpClient, string requestUri, string mediaType)
        {
            var serializedRecord = SerializeObject(record, mediaType);
            var request = BuildRequestMessage(HttpMethod.Put, requestUri, mediaType, mediaType, serializedRecord);
            return SendRequestAsync(httpClient, request);
        }


        /// <summary>
        /// Delete the record at the specified endpoint
        /// </summary>
        /// <param name="httpClient"> 
        /// HttpClient used to send the request. Must be preconfigured and contain the base address and default request headers
        /// </param>
        /// <param name="requestUri">Uri of the target record to delete. Will be combined with the base address specified during httpClient instantiation</param>
        /// <param name="mediaType">Specification of the http media type (e.g. application/json)</param>
        /// <exception cref="">
        /// Throws exceptions if an error occurs
        /// </exception>
        /// <returns>
        /// Task of type HttpResponseMessage containing the raw response from the endpoint 
        /// </returns>
        protected virtual Task<HttpResponseMessage> DeleteAsync(HttpClient httpClient, string requestUri, string mediaType)
        {
            var request = BuildRequestMessage(HttpMethod.Delete, requestUri, mediaType, mediaType);

            //Requests to the Azure Table API require the "If-Match" field as per OData protocol
            //SOURCE : https://www.odata.org/getting-started/basic-tutorial/
            request.Headers.Add("If-Match", "*");

            return SendRequestAsync(httpClient, request);
        }


    }//class
}//namespace