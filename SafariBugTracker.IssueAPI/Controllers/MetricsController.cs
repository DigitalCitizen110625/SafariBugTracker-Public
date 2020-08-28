using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SafariBugTracker.IssueAPI.Models;
using SafariBugTracker.IssueAPI.Services;
using Serilog;

namespace SafariBugTracker.IssueAPI.Controllers
{
    /// <summary>
    /// Receives all incoming requests for providing metrics for a specific project
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        #region Fields, Properties, Constructors



        /// <summary>
        /// Service used to perform all CRUD operations on the incoming issues
        /// </summary>
        private readonly IDatabaseService _dbService;

        /// <summary>
        /// Default logger service
        /// </summary>
        private readonly ILogger<MetricsController> _logger;

        public MetricsController(IDatabaseService dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException($"{nameof(dbService)} cannot be null");
        }



        #endregion



        /// <summary>
        /// Gets a series of metrics used for detailing the performance of the target team
        /// </summary>
        /// <param name="team">Used to calculate the metrics for the supplied team name </param>
        /// <returns>Complete DashboardKPI model with all properties filled </returns>
        [HttpHead]
        [HttpGet("{project}")]
        public async Task<ActionResult<DashboardKPIViewModel>> GetMetrics(string project)
        {
            //Check if the consumer provided a valid collection of IDs
            //Also make sure the ArrayModelBinder was successful
            if (string.IsNullOrEmpty(project))
            {
                return BadRequest(nameof(project) + "parameter was not supplied");
            }

            try
            {
                var dashboardKPI = new DashboardKPIViewModel();
                dashboardKPI = await _dbService.GetLifelongMetrics(dashboardKPI, project);
                dashboardKPI = await _dbService.GetMonthlyMetrics(dashboardKPI, project);
                dashboardKPI = await _dbService.GetDailyMetrics(dashboardKPI, project);
                return Ok(dashboardKPI);
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


    }//class
}//namespace