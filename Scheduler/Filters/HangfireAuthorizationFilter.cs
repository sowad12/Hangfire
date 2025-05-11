using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Scheduler.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public HangfireAuthorizationFilter()
        {
        }

        public bool Authorize([NotNull] DashboardContext context)
        {
            //var httpContext = ((AspNetCoreDashboardContext)context).HttpContext;

            //var httpContext = context.GetHttpContext();
            //var result = httpContext.User.Claims.Any(c => c.Type == ClaimTypes.AppTypeKey) && httpContext.User.Claims.Any(c => c.Type == ClaimType.AppTypeKey && c.Value == ClaimType.AppType.SuperAdmin);

            //Log.Info($"Super Admin has apprequirements: {result}");

            //return result;
            return true;
        }
    }
}
