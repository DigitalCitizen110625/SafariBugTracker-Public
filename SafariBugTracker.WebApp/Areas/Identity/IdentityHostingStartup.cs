using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SafariBugTracker.WebApp.Areas.Identity.Data;
using SafariBugTracker.WebApp.Data;

[assembly: HostingStartup(typeof(SafariBugTracker.WebApp.Areas.Identity.IdentityHostingStartup))]
namespace SafariBugTracker.WebApp.Areas.Identity
{

    /// <summary>
    /// Responsible for configuring identity server. 
    /// </summary>
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            //Config EF Core to use WebAppContext as the default dbContext, and get the connection string from the 
            //  SafariBugTrackerWebAppContextConnection heading in the appsettings file
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<WebAppContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("SafariBugTrackerWebAppContextConnection")));


                //Tell the Identity sub-system to use the auto generated UserContext and WebAppContext 
                //  classes along with EF Core for all db related operations
                services.AddIdentity<UserContext, IdentityRole>(options =>
                {
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.SignIn.RequireConfirmedAccount = false;
                }).AddEntityFrameworkStores<WebAppContext>();
            });
        }
    }
}