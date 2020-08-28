using System;
using SafariBugTracker.WebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SafariBugTracker.WebApp.Areas.Identity.Data;
using System.Threading.Tasks;
using SafariBugTracker.WebApp.Models;
using Microsoft.AspNetCore.Authorization;
using SafariBugTracker.WebApp.Helpers;
using static SafariBugTracker.WebApp.Models.UIAlert;

namespace SafariBugTracker.WebApp.Controllers
{

    /// <summary>
    /// Responsible for handling all routing and logic required for the Dashboard views
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {

        #region Fields, Properties, and Constructors

        /// <summary>
        /// Service responsible for managing user accounts
        /// </summary>
        private readonly UserManager<UserContext> _userManager;

        /// <summary>
        /// Service responsible for managing Identity roles
        /// </summary>
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Service responsible for executing all CRUD operations related to issue entities
        /// </summary>
        private readonly IIssueRepositoryService _issueRepoService;

        /// <summary>
        /// Service responsible for building the query uri to the Issue Api
        /// </summary>
        private readonly QueryBuilderService _queryBuilderService;

        public DashboardController(UserManager<UserContext> userManager, RoleManager<IdentityRole> roleManager, IIssueRepositoryService issueService, QueryBuilderService queryBuilderService)
        {
            _userManager = userManager ?? throw new ArgumentNullException($" {nameof(userManager)} cannot be null");
            _roleManager = roleManager ?? throw new ArgumentNullException($" {nameof(roleManager)} cannot be null");
            _issueRepoService = issueService ?? throw new ArgumentNullException($" {nameof(issueService)} cannot be null");
            _queryBuilderService = queryBuilderService ?? throw new ArgumentNullException($" {nameof(queryBuilderService)} cannot be null");
        }



        #endregion
        #region GetMethods



        /// <summary>
        /// Prints the default view for the dashboard home page, and populates each of the graphs with the data returned from the Issue API. 
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //Get the team designation for the logged in user, and find all users with the same team
            var userContext = await _userManager.GetUserAsync(User);
            if (string.IsNullOrEmpty(userContext.Project))
            {
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Info, "Looks like you aren't part of a project yet. Please fill in the \"Project\" field in your profile"));
                return View(null);
            }

            var dashboardMetricsRequest = await _issueRepoService.GetProjectMetrics(userContext.Project);

            if (dashboardMetricsRequest.dasboardKPI == null)
            {
                UIAlertHelper.SetAlertNotification(this, new UIAlert(AlertType.Error, dashboardMetricsRequest.error));
                return View(null);
            }

            return View(dashboardMetricsRequest.dasboardKPI);
        }


        /// <summary>
        /// Prints the default view for the Submit section of dashboard
        /// </summary>
        [HttpGet]
        public new IActionResult NotFound() => View();


        /// <summary>
        /// Prints the default view for the Settings page of dashboard
        /// </summary>
        [HttpGet]
        public IActionResult Settings() => View();


        /// <summary>
        /// Prints the default view for the Help page of dashboard
        /// </summary>
        [HttpGet]
        public IActionResult Help() => View();

        #endregion
    }//class
}//namespace