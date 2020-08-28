using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SafariBugTracker.LoggerAPI.Helpers
{

    /// <summary>
    /// Responsible for performing a health check on the Azure blob storage endpoint
    /// </summary>
    public class AzureBlobStorageHealthCheck : IHealthCheck
    {

        public AzureBlobStorageHealthCheck()
        {
            //TODO
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context">HealthCheckContext context in which the health check is performed </param>
        /// <param name="cancellationToken">CancellationToken allowing the task to be canceled</param>
        /// <returns> Task of type HealthCheckResult indicating the result of the health check</returns>
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            //TODO
            return null;
        }

    }//class
}//namespace