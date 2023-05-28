using Hangfire.Dashboard;

namespace EndPoint;

public class HangfireAllowAnonymousFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
    }
}
