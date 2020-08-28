using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SafariBugTracker.LoggerAPI.Models;
using SafariBugTracker.LoggerAPI.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SafariBugTracker.LoggerAPI.Helpers;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

namespace LoggerAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            #region Configuration



            services.Configure<LogSettings>(Configuration.GetSection("LogSettings"));
            services.Configure<AuthorizationSettings>(Configuration.GetSection("AuthSettings"));



            #endregion
            #region Services


            services.AddScoped<ILoggerService, LoggerService>();


            #endregion
            #region Security



            var allowedOrigins = Configuration.GetValue<string>("AllowedOrigins")?.Split(",") ?? new string[0];
            services.AddCors(options => options.AddPolicy("SafariInternal", builder => builder.WithOrigins(allowedOrigins)));



            #endregion
            #region HealthChecks


            //Readiness/dependency health checks for the mongodb issue database
            var databaseConnectionString = Configuration.GetSection("DatabaseSettings")["ConnectionString"];
            services.AddHealthChecks().AddCheck("Log Storage Health Check", new AzureBlobStorageHealthCheck(), HealthStatus.Unhealthy, tags: new[] { "ready" });



            #endregion
            #region RequestFormat


            services.AddControllers(setupAction =>
            {
                //The app only returns data in the default format (i.e. Json). Lets change that to allow 
                //  returns of other formats (e.g. xml) if requested by the client
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
            });


            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //When in the development environment all the return of uncaught exceptions
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //When deployed to production mode, set the app to automatically hide 
                //  all unhandled exceptions
                app.UseExceptionHandler(appBuilder =>
                {
                    //If a request results in an uncaught exception, set the default return message to 
                    //  a status 500 Internal Server Error, with the following  string
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault occurred. Please try again later");
                    });
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("SafariInternal");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
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