using Hystrix.Dashboard.Logging;
using Microsoft.Owin.Hosting;
using System;

namespace Hystrix.Dashboard
{
    public static class HystrixDashboard
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(OwinConfig));

        static IDisposable app;

        public static void Selfhost(string address)
        {
            if (app != null)
                throw new InvalidOperationException("Hysrix Dashboard is already hosted. HystrixDashboard.Selfhost can be called only once");
            Logger.Info($"Started dashboard on {address}");
            var listener = typeof(Microsoft.Owin.Host.HttpListener.OwinHttpListener);
            app = WebApp.Start<OwinConfig>(url: address);

        }
        public static void Stop()
        {
            if (app != null)
                app.Dispose();
        }
    }
}
