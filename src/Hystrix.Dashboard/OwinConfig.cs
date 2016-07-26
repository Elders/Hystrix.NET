using Hystrix.Dashboard.Logging;
using Hystrix.Dashboard.StaticFiles;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace Hystrix.Dashboard
{
    public class OwinConfig
    {
        private HystrixProxyStream proxyStream;

        private static readonly ILog Logger = LogProvider.GetLogger(typeof(OwinConfig));

        public void Configuration(IAppBuilder appBuilder)
        {
            var staticFilesNamespace = typeof(StaticFilesNameSpace).Namespace;
            var assembly = typeof(OwinConfig).Assembly.GetManifestResourceNames();

            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                RequestPath = new PathString("/dashboard"),
                FileSystem = new EmbeddedResourceFileSystem(typeof(OwinConfig).Assembly, staticFilesNamespace)
            };
            proxyStream = new HystrixProxyStream();
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            options.DefaultFilesOptions.DefaultFileNames = new[] { "index.html" };
            appBuilder.Use((context, next) =>
            {
                proxyStream.ProcessRequest(context);
                return next.Invoke();
            });
            appBuilder.UseFileServer(options);
        }
    }
}