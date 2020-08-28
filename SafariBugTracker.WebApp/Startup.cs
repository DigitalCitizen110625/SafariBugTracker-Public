using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafariBugTracker.WebApp.Helpers;
using SafariBugTracker.WebApp.Models;
using SafariBugTracker.WebApp.Models.Settings;
using SafariBugTracker.WebApp.Services;
using Serilog;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SafariBugTracker.WebApp
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


            services.Configure<IssueApiKey>(Configuration.GetSection(nameof(IssueApiKey)));
            services.Configure<AzureTableSettings>(Configuration.GetSection(nameof(AzureTableSettings)));
            services.Configure<IssueRepositorySettings>(Configuration.GetSection(nameof(IssueRepositorySettings)));
            services.Configure<ImageUploadSettings>(Configuration.GetSection(nameof(ImageUploadSettings)));
            services.Configure<SmtpEmailSettings>(Configuration.GetSection(nameof(SmtpEmailSettings)));
            services.Configure<SqlSettings>(Configuration.GetSection("ConnectionStrings"));


            #endregion
            #region Services


            services.AddControllersWithViews();
            services.AddSingleton<IIssueRepositoryService, IssueRepositoryService>();   //Repository service for issues/bugs
            services.AddSingleton<NoteRepositoryService>();                             //Repository service for user notes
            services.AddSingleton<ImageProcessingService>();                            //Service for ensuring all image uploads meet the same standards
            services.AddSingleton<EmailService>();                                      //Service for sending all SMTP emails over AWS simple email service
            services.AddScoped<QueryBuilderService>();                                  //Service for building uri queries to the issue service
            //services.AddScoped<IEmailSender, EmailSender>();

            #endregion
            #region HealthChecks


            //Readiness/dependency health checks for the identity database
            var connectionString = Configuration.GetConnectionString("SafariBugTrackerWebAppContextConnection");
            services.AddHealthChecks().AddCheck("Sql Server Health Check", new SqlServerHealthCheck(connectionString), HealthStatus.Unhealthy, tags: new[] { "ready" });


            #endregion
            #region IdentityServer


            //Note: See the IdentityHostingStartup class for the remainder of the identity configuration
            //The scaffolded identity pages aren't in MVC format, and use regular ASP.NET Core razor pages
            services.AddRazorPages();


            #endregion
            #region Miscellaneous


            //services.AddCors(options => options.AddPolicy("AllowEverything", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));


            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            //app.UseCors("AllowEverything");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");


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
                        [HealthStatus.Unhealthy] =StatusCodes.Status503ServiceUnavailable
                    },

                    //Set the contents of the response based on the result of the check
                    ResponseWriter = WriteHealthCheckReadyResponse,

                    //Check the request to ensure it has the ready tag
                    Predicate = (check) => check.Tags.Contains("ready"),
                    AllowCachingResponses = false
                });


                #endregion


                //Required for Identity
                endpoints.MapRazorPages();                              
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

    }//class
}//namespace