using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafariBugTracker.WebApp.Areas.Identity.Data;
using SafariBugTracker.WebApp.Helpers;
using SafariBugTracker.WebApp.Models;
using SafariBugTracker.WebApp.Models.ViewModels;
using SafariBugTracker.WebApp.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SafariBugTracker.WebApp.Models.UIAlert;

namespace SafariBugTracker.WebApp.Controllers
{

    /// <summary>
    ///  Responsible for handling all routing, logic, and CRUD operations originating from the bug/issue related views
    /// </summary>
    [Authorize]
    public class IssueController : Controller
    {

        #region Fields, Properties, Constructor



        /// <summary>
        /// Service responsible for executing all CRUD operations related to issue entities
        /// </summary>
        private readonly IIssueRepositoryService _issueRepoService;

        /// <summary>
        /// Service responsible for building the query uri to the Issue Api
        /// </summary>
        private readonly QueryBuilderService _queryBuilderService;

        /// <summary>
        /// Service responsible for managing user accounts
        /// </summary>
        private readonly UserManager<UserContext> _userManager;

        /// <summary>
        /// Service responsible for performing validation checks on uploaded images
        /// </summary>
        private readonly ImageProcessingService _imageProcessor;

        public IssueController(IIssueRepositoryService issueService, 
            NoteRepositoryService noteService, 
            QueryBuilderService queryBuilderService, 
            UserManager<UserContext> userManager, ImageProcessingService imageProcessor)
        {
            _issueRepoService = issueService ?? throw new ArgumentNullException($" {nameof(IIssueRepositoryService)} cannot be null");
            _queryBuilderService = queryBuilderService ?? throw new ArgumentNullException($" {nameof(QueryBuilderService)} cannot be null");
            _userManager = userManager ?? throw new ArgumentNullException($" {nameof(userManager)} cannot be null");
            _imageProcessor = imageProcessor ?? throw new ArgumentNullException($" {nameof(imageProcessor)} cannot be null");
        }



        #endregion
        #region GetMethods



        /// <summary>
        /// Prints the default view for the  MyReports section of dashboard
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UserIssues(int? pageIndex, int? pageSize)
        {
            _queryBuilderService.AddFilterParameter("keyword", _userManager.GetUserId(User));
            _queryBuilderService.ConstructQuery();


            var result = await _issueRepoService.QueryIssues(_queryBuilderService.QueryString);

            var issues = Enumerable.Empty<IIssue>().AsQueryable();
            var model = PaginatedList<IIssue>.Create(issues.AsNoTracking(), pageIndex.GetValueOrDefault(), pageSize.GetValueOrDefault());
            if (result.error != null && result.issues == null)
            {
                //Error encountered
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, result.error));
                return View(model);
            }
            else if (result.issues == null)
            {
                //No errors encountered, but the query yielded no results
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Info, "Query Yielded No Results"));
                return View(model);
            }

            issues = result.issues.AsQueryable();

            //Default the pageSize, and pageIndex if not specified by the user
            const int defaultPageIndex = 1;
            const int defaultRecordsPerPage = 10;

            pageSize ??= defaultRecordsPerPage;
            pageIndex ??= defaultPageIndex;
            model = PaginatedList<IIssue>.Create(issues.AsNoTracking(), pageIndex.GetValueOrDefault(), pageSize.GetValueOrDefault());
            return View(model);
        }


        /// <summary>
        /// Prints the default view for the searching feature, and auto queries the database for the
        /// most recently submitted issues. The first time the page is displayed, or if the user hasn't clicked a paging or sorting link, all the parameters 
        /// will be null. If a paging link is clicked, the page variable will contain the page number to display
        /// </summary>
        /// <param name="pageIndex">String indicating the current index position in the paginated list</param>
        /// <param name="pageSize">string indicating the max count of elements (rows) in the paginated list</param>
        /// <param name="keyword">String to use in the keyword search operation </param>
        /// <param name="id">String ID of the desired record</param>
        /// <param name="author">String used to find records with a matching author designation </param>
        /// <param name="assignedTo">String used to find records who've been assigned to the specified user/account</param>
        /// <param name="platform">String used to find records with a matching platform designation</param>
        /// <param name="product">String used to find records with a matching product designation</param>
        /// <param name="category"String used to find records with a matching category designation></param>
        /// <param name="startDate">String used to find records with a matching date (unless otherwise specified in the dateSearchType param)</param>
        /// <param name="endDate">String used to find records with a matching date (unless otherwise specified in the dateSearchType param) </param>
        /// <param name="dateSearchType">String indicating the logical comparison to use for finding records with eq, gt, or le the specified date </param>
        /// <param name="startVersion">String used to find records with a matching version number (unless otherwise specified in the versionSearchType param) </param>
        /// <param name="endVersion">String used to find records with a matching version number (unless otherwise specified in the versionSearchType param) </param>
        /// <param name="versionSearchType">String indicating the logical comparison to use for finding records with eq, gt, or le the specified version number </param>
        /// <param name="resolveStatus">String used to find records with a resolve status designation</param>
        /// <returns>Paginated list containing all the records that matched the search results</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Search(
            int? pageIndex,
            int? pageSize,
            string keyword,
            string id,
            string author,
            string assignedTo,
            string platform,
            string product,
            string category,
            string startDate,
            string endDate,
            string dateSearchType,
            string startVersion,
            string endVersion,
            string versionSearchType,
            string resolveStatus)
        {
            _queryBuilderService.AddFilterParameter(nameof(keyword), keyword);
            _queryBuilderService.AddFilterParameter(nameof(id), id);
            _queryBuilderService.AddFilterParameter(nameof(author), author);
            _queryBuilderService.AddFilterParameter(nameof(assignedTo), assignedTo);
            _queryBuilderService.AddFilterParameter(nameof(platform), platform);
            _queryBuilderService.AddFilterParameter(nameof(product), product);
            _queryBuilderService.AddFilterParameter(nameof(category), category);
            _queryBuilderService.AddFilterParameter(nameof(resolveStatus), resolveStatus);


            //Date filter param
            if (dateSearchType == "Before")
            {
                _queryBuilderService.AddFilterParameter(nameof(startDate), startDate, ODataFilter.ComparisonOperator.lt);
            }
            else if (dateSearchType == "After")
            {
                _queryBuilderService.AddFilterParameter(nameof(startDate), startDate, ODataFilter.ComparisonOperator.gt);
            }
            else if (dateSearchType == "Between")
            {
                _queryBuilderService.AddFilterParameter(nameof(startDate), startDate, ODataFilter.ComparisonOperator.gt);
                _queryBuilderService.AddFilterParameter(nameof(endDate), endDate, ODataFilter.ComparisonOperator.lt);
            }
            else
            {
                _queryBuilderService.AddFilterParameter(nameof(startDate), startDate);
            }

            //Version filter param
            if (versionSearchType == "Before")
            {
                _queryBuilderService.AddFilterParameter(nameof(startVersion), startVersion, ODataFilter.ComparisonOperator.lt);
            }
            else if (dateSearchType == "After")
            {
                _queryBuilderService.AddFilterParameter(nameof(startVersion), startVersion, ODataFilter.ComparisonOperator.gt);
            }
            else if (dateSearchType == "Between")
            {
                _queryBuilderService.AddFilterParameter(nameof(startVersion), startVersion, ODataFilter.ComparisonOperator.gt);
                _queryBuilderService.AddFilterParameter(nameof(endVersion), endVersion, ODataFilter.ComparisonOperator.lt);
            }
            else
            {
                _queryBuilderService.AddFilterParameter(nameof(endVersion), endVersion);
            }

            _queryBuilderService.ConstructQuery();
            var result = await _issueRepoService.QueryIssues(_queryBuilderService.QueryString);


            //Default the return model in case there was an error, or no results
            var issues = Enumerable.Empty<IIssue>().AsQueryable();
            var model = PaginatedList<IIssue>.Create(issues.AsNoTracking(), pageIndex.GetValueOrDefault(), pageSize.GetValueOrDefault());

            if (result.error != null && result.issues == null)
            {
                //Error encountered
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, result.error));
                return View(model);
            }
            else if(result.issues == null)
            {
                //No errors encountered, but the query yielded no results
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Info, "Query Yielded No Results"));
                return View(model);
            }
 
            //No errors encountered, and the search matches some of the records
            issues = result.issues.AsQueryable();

            const int defaultPageIndex = 1;         //Default index if not changed by the user
            const int defaultRecordsPerPage = 10;   //Default page size

            pageSize ??= defaultRecordsPerPage;
            pageIndex ??= defaultPageIndex;
            model = PaginatedList<IIssue>.Create(issues.AsNoTracking(), pageIndex.GetValueOrDefault(), pageSize.GetValueOrDefault());
            return View(model);

        }


        /// <summary>
        /// Prints the default view for the Submit section of dashboard
        /// </summary>
        [HttpGet]
        [Authorize]
        public IActionResult Submit() => View();


        /// <summary>
        /// Prints the default view containing the details of the selected record
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("NotFound");
            }

            var result = await _issueRepoService.GetIssue(id);
            if (result.issue == null)
            {
                //Error encountered
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, result.error));
                return View();
            }
            return View(result.issue);
        }


        /// <summary>
        /// Prints the Edit view, finds a record with a matching id, and populates the view with the records details
        /// </summary>
        /// <param name="id">Id used to find the record</param>
        /// <returns>Task indicating the result of the operation</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                //ID must be explicitly specified
                return RedirectToAction("NotFound");
            }

            var result = await _issueRepoService.GetIssue(id);
            if (result.issue == null)
            {
                //Record with a matching ID was not found
                return RedirectToAction("NotFound");
            }
            return View(result.issue);
        }




        #endregion
        #region PostMethods



        /// <summary>
        /// Ensures the model is valid, and passes the updated details to the api service. 
        /// </summary>
        /// <param name="issue">Issue object that can be edited </param>
        /// <returns> Reprints the page with the results of the edit, or an alert indicating if the submission has any errors</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(Issue issue, string message)
        {
            if (!ModelState.IsValid)
            {
                //User filled in the form incorrectly -> reload the page and print the errors
                return View(issue);
            }

            //Check if a new message was posted to the issue, if it was, get the posters details,
            //  and save them along with the contents of the message
            if(!string.IsNullOrEmpty(message))
            {
                var userContext = await _userManager.GetUserAsync(User);
                var userPost = new Message();

                userPost.PosterUserID = userContext.Id;
                userPost.PosterDisplayName = userContext.DisplayName;
                userPost.PostDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                userPost.MessageContent = message;
                if(issue.Messages == null)
                {
                    issue.Messages = new List<Message>();
                }
                issue.Messages.Add(userPost);
            }

            var result = await _issueRepoService.UpdateIssue(issue);
            var alertType = result.success ? AlertType.Success : AlertType.Error;
            UIAlertHelper.SetAlertNotification(this, new UIAlert(alertType, result.message));

            return RedirectToAction("Edit", "Issue", new { id = issue.Id });
        }


        /// <summary>
        /// Ensures the submission is valid, and sends the new issue to the
        /// API to be saved to the database
        /// </summary>
        /// <param name="issue"> Issue object containing the details of the submitted record </param>
        /// <param name="image"> Uploaded image file to be added to the issue record </param>
        /// <returns> Reprints the page with an alert indicating if the submission was a success </returns>
        [HttpPost]
        public async Task<IActionResult> Submit(Issue issue, IFormFile image)
        {
            if (!ModelState.IsValid)
            {
                //User filled in the form incorrectly -> reload the page and print the errors
                return View(issue);
            }

            if (image != null)
            {
                if (!_imageProcessor.IsFileSizeValid(image.Length))
                {
                    //image size was above the max allowable limit
                    UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "File size cannot exceed 12 mb"));
                    return View(issue);
                }

                if (!_imageProcessor.IsImageTypeValid(image.ContentType))
                {
                    //image file type was not among the list of accepted mime types
                    UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, "File type must be an image (e.g. bmp, gif, jpeg, png, svg, webp, tiff"));
                    return View(issue);
                }

                //No errors detected
                issue.ContentType = image.ContentType;
                issue.Image = SafariBugTracker.WebApp.Helpers.TypeConverter.ConvertToBytes(image);
            }

            issue.UpdatedDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            issue.ResolveStatus = "New";
            issue.Messages = new List<Message>();

            var result =  await _issueRepoService.InsertIssue(issue);
            var alertType = result.success ? AlertType.Success : AlertType.Error;
            UIAlertHelper.SetAlertNotification(this, new UIAlert(alertType, result.message));

            return RedirectToAction("Submit", "Issue");
        }


        /// <summary>
        /// Passes the details of the issue and delete command to the api service
        /// </summary>
        /// <param name="id">id of the record to be deleted</param>
        /// <returns>Reprints the page with the results of the delete operation </returns>
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Search");
            }
            var result = await _issueRepoService.DeleteIssue(id);

            if (result.success == true)
            {
                return RedirectToAction("Search");
            }
            else
            {
                //Error encountered during delete, pass the error message, and return to the view
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, result.message));
                return View();
            }
        }



        #endregion
    }//class
}//namespace