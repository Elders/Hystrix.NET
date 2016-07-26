using Owin;

namespace Hystrix.Dashboard
{
    public static class IAppBuilderExtensions
    {
        public static void UseHystrixDashboard(this IAppBuilder appBuilder)
        {
            var dashboard = new OwinConfig();
            dashboard.Configuration(appBuilder);
        }
    }
}
