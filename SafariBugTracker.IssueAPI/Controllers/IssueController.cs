using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafariBugTracker.IssueAPI.Models;
using SafariBugTracker.IssueAPI.Services;
using System.Collections.Generic;
using SafariBugTracker.IssueAPI.Helpers;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Extensions.Logging;

namespace SafariBugTracker.IssueAPI.Controllers
{

    /// <summary>
    /// Receives all incoming requests related to the submission, finding, updating, and deleting issues.
    /// No responsible for performing any CRUD operations, but instead passes the request details to the DatabaseService.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        #region Properties


        /// <summary>
        /// Service used to perform all CRUD operations on the incoming issues
        /// </summary>
        private readonly IDatabaseService _dbService;

        /// <summary>
        /// Service used to validate, deconstruct, and convert queries into a format understandable by the database
        /// </summary>
        private readonly MongoQueryConverterService _queryService;


        public IssuesController(IDatabaseService dbService, MongoQueryConverterService queryService)
        {
            _dbService = dbService ?? throw new ArgumentNullException($"{nameof(dbService)} cannot be null");
            _queryService = queryService ?? throw new ArgumentNullException($"{nameof(queryService)} cannot be null");
        }


        #endregion
        #region PrivateMethods


        /// <summary>
        /// Checks that the id has a valid length, and character composition to match a mongodb ObjectId.
        /// See the documentation for further details https://docs.mongodb.com/manual/reference/method/ObjectId/
        /// </summary>
        /// <param name="id"> The string id to check</param>
        /// <returns>True if the id string matches the regex pattern, false otherwise</returns>
        private bool IsIDValid(string id)
        {
            //Match any chars between a - f, or a digit, and 24 chars in length
            Regex rx = new Regex(@"^[a-f\d]{24}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = rx.Matches(id);
            if (matches.Count > 0)
            {
                return true;
            }
            return false;
        }


        #endregion
        #region PublicMethods


        /// <summary>
        /// Retrieves every record from the database
        /// </summary>
        /// <remarks>Route: /api/issues/</remarks>
        /// <returns>Collection of all issues from the currently selected database </returns>
        [HttpHead]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IIssue>>> Get() => await _dbService.Find();


        /// <summary>
        /// Queries the database for a record a matching id. If a match is found, 
        /// the object is returned
        /// </summary>
        /// <remarks>Route: /api/issues/objectID</remarks>
        /// <param name="id"> Id used to locate the matching record </param>
        /// <returns> HTTP status code and object with matching ID, or error message </returns>
        [HttpHead]
        [HttpGet("{id:length(24)}")]
        public ActionResult<IIssue> Get(string id)
        {
            //Note that the default mongodb objectID length is 24

            //Check if the consumer provided a complete id
            if (!IsIDValid(id))
            {
                return BadRequest("Ensure all parameters are supplied and non-null");
            }

            try
            {
                var issue = _dbService.Find(id);
                if (issue == null)
                {
                    return NotFound($"Resource: {id} was not found");
                }
                return Ok(issue);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Searches the database for a collection of records matching the parameters 
        /// and options specified in the query string
        /// </summary>
        /// <remarks>Route: /api/issues/search?{filter}</remarks>
        /// <param name="filter">String of the query specifics</param>
        /// <returns>Collection of issues that matched the query</returns>
        [HttpHead]
        [HttpGet("{filter}")]
        [Route("search")]
        public async Task<ActionResult<IEnumerable<IIssue>>> Search([FromQuery] string filter)
        {
            //Check if the consumer provided a complete id
            if (string.IsNullOrEmpty(filter))
            {
                return await Get();
            }
            try
            {

                var queryParams = _queryService.ExtractQueryParameters(filter);
                var issues = await _dbService.Find(queryParams);
                if (issues == null)
                {
                    return NotFound($"None of the requested resources could be found");
                }
                return Ok(issues);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Takes the provided ids, and queries the db for any matching entities with a matching id field
        /// </summary>
        /// <remarks> Route: api/issue/(objectID1,objectID2,etc.)</remarks>
        /// <param name="issueIds">Id of the records to find</param>
        /// <returns>Collection of entities with ids matching those provided in the uri</returns>
        [HttpHead]
        [HttpGet("({issueIds})", Name = "GetIssueCollection")]
        public IActionResult Get([FromRoute] [ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<string> issueIds)
        {
            //Check if the consumer provided a valid collection of IDs
            //Also make sure the ArrayModelBinder was successful
            if (issueIds == null)
            {
                return BadRequest();
            }

            try
            {
                var issues = _dbService.Find(issueIds);
                if (issues == null)
                {
                    return NotFound($"None of the requested resources could be found");
                }
                return Ok(issues);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Takes the details of the incoming request, and passes them to the database issue for retrieval. 
        /// Note: This method has a different route in order to avoid the "multiple matching endpoints" error when
        /// posting a collection of key-value pairs to the controller
        /// </summary>
        /// <remarks> 
        /// Route: api/issue/GetIssueCollection where the body is constructed in the following manner: 
        /// ["ObjecyId1", "ObjecyId2", "ObjecyId3"]
        /// Reference: https://stackoverflow.com/questions/39667294/how-to-post-string-array-using-postman
        /// </remarks>
        /// <returns>Collection of entities with matching ids provided in the request body</returns>
        [HttpPost]
        [Route("GetIssueCollection")]
        public IActionResult GetIssues([FromBody] IEnumerable<string> issueIds)
        {
            //Create a list of valid ids
            var validIDs = new List<string>();
            foreach (var id in issueIds)
            {
                if(!IsIDValid(id))
                {
                    validIDs.Add(id);
                }
            }

            try
            {
                //Query the database for the matching ids
                var issues = _dbService.Find(validIDs);
                if (issues == null)
                {
                    return NotFound($"None of the requested resources could be found");
                }
                return Ok(issues);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Takes the details of the incoming request, and saves them to the database
        /// </summary>
        /// <remarks> Route: api/issue</remarks>
        /// <param name="issue"> Issue details from the message body </param>
        /// <returns> HTTP status code and descriptor message indicating the result </returns>
        [HttpPost]
        public async Task<ActionResult<Issue>> Submit([FromBody]Issue issue)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest("Ensure all parameters are supplied and non-null");
            }

            try
            {
                //Submit the entity for creation in the database
                issue.SubmissionDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                issue.UpdatedDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                await _dbService.Create(issue);
                return Ok($"Resource: {issue.Id} submitted successfully");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Receives an HttpRequest, and passes the id string to the database service, where
        /// an entity with the matching id field will be updated
        /// </summary>
        /// <remarks> Route: api/issue/objectID</remarks>
        /// <param name="id">ID field of the entity to update</param>
        /// <param name="updatedIssue">Contains the updates values for the target entity</param>
        /// <returns>HttpResposne with status code, and response string indicating the result of the operation</returns>
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, [FromBody] Issue updatedIssue)
        {
            //Check if the consumer provided a complete id
            if (!IsIDValid(id))
            {
                return BadRequest("The uri issue id was invalid, or not provided");
            }


            if (!ModelState.IsValid)
            {
                return BadRequest("Ensure all parameters are supplied and non-null");
            }

            try
            {
                //Ensure an entity with a matching id already exists;
                if (_dbService.Find(id) == null)
                {
                    return NotFound($"Resource: {id} was not found");
                }

                //Find the matching entity and update all its properties to the new values
                updatedIssue.UpdatedDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                var result = await _dbService.Update(id, updatedIssue);
                return Ok($"Resource: {id} updated successfully");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Receives an HttpRequest, and passes the id string to the database service, where
        /// an entity with the matching id field will be deleted
        /// </summary>
        /// <remarks> Route: api/issue/objectID</remarks>
        /// <param name="id">ID field of the entity to delete</param>
        /// <returns>HttpResposne with status code, and response string indicating the result of the operation</returns>
        [HttpDelete("{id:length(24)}")]
        public IActionResult Delete(string id)
        {
            //Check if the consumer provided a complete id
            if (!IsIDValid(id))
            {
                return BadRequest("Ensure all parameters are supplied and non-null");
            }

            try
            {
                //Ensure an entity with a matching id already exists
                if (_dbService.Find(id) == null)
                {
                    return NotFound($"Resource: {id} was not found");
                }

                //Find the matching entity and delete it
                _dbService.Remove(id);
                return Ok($"Resource: {id} removed successfully");
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            } 
        }





        #endregion
    }//class
}//namespace