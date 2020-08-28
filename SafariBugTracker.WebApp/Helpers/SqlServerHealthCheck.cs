using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SafariBugTracker.WebApp.Helpers
{

    /// <summary>
    /// Performs a quick check to see if a connection can be established to the sql server database
    /// </summary>
    public class SqlServerHealthCheck : IHealthCheck
    {
        /// <summary>
        /// Connection string used to connect to the database
        /// </summary>
        private readonly string _connectionString;

        public SqlServerHealthCheck(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Performs a health check on the sql server by trying to establish a connection to the endpoint specified by the connection string
        /// </summary>
        /// <param name="context">HealthCheckContext context in which the health check is performed </param>
        /// <param name="cancellationToken">CancellationToken allowing the task to be canceled</param>
        /// <returns> Task of type HealthCheckResult indicating the result of the health check</returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    return Task.FromResult(HealthCheckResult.Healthy());
                }
            }
            catch (Exception e)
            {
                //TODO: Find a way to stop the identity system from allowing logins/registrations if the db health check comes back as poor
                switch (context.Registration.FailureStatus)
                {
                    //Current test is binary as you can either access the db (healthy) or not (Unhealthy)
                    //case HealthStatus.Degraded:
                    //    return Task.FromResult(HealthCheckResult.Degraded($"Issues opening connection to SQL server", e));
                    default:
                        Log.Error(e, "SQL HealthCheckResult returned: Unhealthy due to" + e.Message);
                        return Task.FromResult(HealthCheckResult.Unhealthy($"Issues opening connection to SQL server", e));
                }
            }
        }

    }//class
}//namespace