using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafariBugTracker.IssueAPI.Helpers;
using SafariBugTracker.IssueAPI.Middleware;
using SafariBugTracker.IssueAPI.Models;
using SafariBugTracker.IssueAPI.Services;
using System.Linq;
using System.Threading.Tasks;

namespace BugTrackerAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {

            #region Configuration


            services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(DatabaseSettings)));
            services.Configure<AuthenticationMiddlewareSettings>(Configuration.GetSection("AuthSettings"));


            #endregion
            #region Services



            //Allows us to inject a reference to the IDatabaseSettings class and access the config options in any service
            services.AddSingleton<IDatabaseSettings>(isp => isp.GetRequiredService<IOptions<DatabaseSettings>>().Value);

            //Register the mongodb service as the primary database manager
            services.AddSingleton<IDatabaseService, DatabaseService>();

            //Register the service to deconstruct incoming queries 
            services.AddSingleton<MongoQueryConverterService>();

            services.AddMemoryCache();



            #endregion
            #region Security



            //var allowedOrigins = Configuration.GetValue<string>("AllowedOrigins")?.Split(",") ?? new string[0];
            //services.AddCors(options => options.AddPolicy("SafariInternal", builder => builder.WithOrigins(allowedOrigins)));



            #endregion
            #region HealthChecks



            //Readiness/dependency health checks for the mongodb issue database
            var issueCollectionName = Configuration.GetSection("DatabaseSettings")["IssueCollectionName"];
            var databaseName = Configuration.GetSection("DatabaseSettings")["DatabaseName"];
            var databaseConnectionString = Configuration.GetSection("DatabaseSettings")["ConnectionString"];
            services.AddHealthChecks().AddCheck("Issue Database Health Check", new MongoDbHealthCheck(issueCollectionName, databaseName, databaseConnectionString), HealthStatus.Unhealthy, tags: new[] { "ready" });



            #endregion
            #region Routing

            services.AddControllers(setupAction =>
            {
                //Currently, the app only returns data in the default format (i.e. Json). Lets change that to allow 
                //  returns of other formats (e.g. xml) if requested by the client
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

            }).AddXmlDataContractSerializerFormatters();
            //.ConfigureApiBehaviorOptions(setupAction =>
            //{
            //    setupAction.InvalidModelStateResponseFactory = context =>
            //    {
            //        //Create a problem details object for all error returns
            //        var problemDetailsFactory = context.HttpContext.RequestServices.
            //        GetRequiredService<ProblemDetailsFactory>();

            //        var problemDetails = problemDetailsFactory.CreateProblemDetails(context.HttpContext, context.ModelState);
            //    };
            //});



            #endregion
            #region IdentityServer







            #endregion
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }    
 
            app.UseHttpsRedirection();

            app.UseMiddleware<AuthenticationMiddleware>();

            app.UseRouting();
            // app.UseRequestLocalization();

            //app.UseCors("SafariInternal");

            app.UseAuthentication();
            app.UseAuthorization();
            // app.UseSession();
            // app.UseResponseCaching()

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                #region HealthChecks


                //Base liveness health check to see if the web application is online or not
                //The custom dependency health checks can be found above as services.AddHealthChecks().AddCheck<Type>()
                //SOURCE: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.1
                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions()
                {
                    //Only run this check if it doesn't contain the ready tag in the request
                    Predicate = (check) => !check.Tags.Contains("ready"),
                    ResponseWriter = WriteHealthCheckLiveResponse,
                    AllowCachingResponses = false
                });

                //Preconfigured health check for checking if the
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
                {
                    //Set the status code of the response based on the result of the check
                    ResultStatusCodes = {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status500InternalServerError,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    },

                    //Set the contents of the response based on the result of the check
                    ResponseWriter = WriteHealthCheckReadyResponse,

                    //Check the request to ensure it has the ready tag
                    Predicate = (check) => check.Tags.Contains("ready"),
                    AllowCachingResponses = false
                });


                #endregion
            });
        }



        /// <summary>
        /// Sends an http response with a new JSON object with a set of predefined properties, 
        /// and values indicating the status of the health check
        /// </summary>
        /// <remarks>
        /// This code was found on a tutorial service on asp.net core health checks
        /// SOURCE: https://www.pluralsight.com/courses/asp-dot-net-core-health-checks
        /// </remarks>
        /// <param name="httpContext">Contains the details of the http GET request used to make the health check</param>
        /// <param name="result">HealthReport result of the health check</param>
        /// <returns>Task of the write  health check status </returns>
        private Task WriteHealthCheckLiveResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";

            var json = new JObject(
                    new JProperty("OverallStatus", result.Status.ToString()),
                    new JProperty("TotalChecksDuration", result.TotalDuration.TotalSeconds.ToString("0:0.00"))
                );

            return httpContext.Response.WriteAsync(json.ToString(Formatting.Indented));
        }


        /// <summary>
        /// Sends an http response with a new JSON object with a set of predefined properties, 
        /// and values indicating the status of the health check
        /// </summary>
        /// <remarks>
        /// This code was found on a tutorial service on asp.net core health checks
        /// SOURCE: https://www.pluralsight.com/courses/asp-dot-net-core-health-checks
        /// </remarks>
        /// <param name="httpContext">Contains the details of the http GET request used to make the health check</param>
        /// <param name="result">HealthReport result of the health check</param>
        /// <returns>Task of the write  health check status </returns>
        private Task WriteHealthCheckReadyResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";
            var json = new JObject(
                    new JProperty("OverallStatus", result.Status.ToString()),
                    new JProperty("TotalChecksDuration", result.TotalDuration.TotalSeconds.ToString("0:0.00")),
                    new JProperty("DependencyHealthChecks", new JObject(result.Entries.Select(dicItem =>
                        new JProperty(dicItem.Key, new JObject(
                                new JProperty("Status", dicItem.Value.Status.ToString()),
                                new JProperty("Duration", dicItem.Value.Duration.TotalSeconds.ToString("0:0.00")),
                                new JProperty("Exception", dicItem.Value.Exception?.Message),
                                new JProperty("Data", new JObject(dicItem.Value.Data.Select(dicData =>
                                    new JProperty(dicData.Key, dicData.Value))))
                            ))
                    )))
                );

            //Send the http response back to the requesting client
            return httpContext.Response.WriteAsync(json.ToString(Formatting.Indented));
        }
    }
}