using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SafariBugTracker.WebApp.Areas.Identity.Data;

namespace SafariBugTracker.WebApp.Data
{
    /// <summary>
    /// Base context class used by EFC when dealing with Identity related data
    /// </summary>
    public class WebAppContext : IdentityDbContext<UserContext>
    {

        public WebAppContext(DbContextOptions<WebAppContext> options): base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
