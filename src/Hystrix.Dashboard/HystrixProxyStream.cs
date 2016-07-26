using Hystrix.Dashboard.Logging;
using Microsoft.Owin;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace Hystrix.Dashboard
{
    [Obsolete("I see no reason why this class should exists. We could do this directly in the javascript...")]
    public class HystrixProxyStream
    {
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(HystrixProxyStream));

        /// <summary>
        /// Continuously transfers the data from the source uri to the client.
        /// The source must be specified in the 'origin' query string parameter.
        /// </summary>
        /// <param name="context">The client request.</param>
        public void ProcessRequest(object owinContext)
        {
            var context = owinContext as OwinContext;

            if (context.Request.Path.HasValue == false || context.Request.Path.Value != "/proxy.stream")
                return;
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.Request.Query["origin"] == null)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("Required query string parameter 'origin' is missing.");
                return;
            }

            Uri sourceUri = GetSourceUri(context.Request);
            Logger.Info(string.Format(CultureInfo.InvariantCulture, "Opening connection to '{0}'.", sourceUri));

            try
            {
                HttpWebRequest sourceRequest = HttpWebRequest.CreateHttp(sourceUri);
                using (HttpWebResponse sourceResponse = (HttpWebResponse)sourceRequest.GetResponse())
                {
                    if (sourceResponse.StatusCode == HttpStatusCode.OK)
                    {
                        context.Response.Headers.Add("Content-Type", new string[] { "text/event-stream;charset=UTF-8" });
                        context.Response.Headers.Add("Cache-Control", new string[] { "no-cache, no-store, max-age=0, must-revalidate" });
                        context.Response.Headers.Add("Pragma", new string[] { "no-cache" });

                        StreamSourceToClient(sourceResponse, context.Response);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorException(string.Format(CultureInfo.InvariantCulture, "Error occurred during streaming from '{0}'.", sourceUri), e);
                throw;
            }
        }

        /// <summary>
        /// Stream everything from the source response to the client response.
        /// </summary>
        /// <param name="sourceResponse">The source to read.</param>
        /// <param name="clientResponse">The client to write.</param>
        private static void StreamSourceToClient(HttpWebResponse sourceResponse, IOwinResponse clientResponse)
        {
            using (StreamReader inputStreamReader = new StreamReader(sourceResponse.GetResponseStream()))
            {
                string line;
                try
                {
                    while ((line = inputStreamReader.ReadLine()) != null)
                    {
                        try
                        {
                            clientResponse.Write(line + Environment.NewLine);

                        }
                        catch (Exception ex)
                        {
                            Logger.Info("Client disconnected.");
                            break;
                        }
                    }
                }
                catch (IOException)
                {
                    Logger.Warn("The metrics source disconnected.");
                }
            }
        }

        /// <summary>
        /// Extracts the target URI from the current request's query string.
        /// </summary>
        /// <param name="request">The current request.</param>
        /// <returns>The target URI.</returns>
        private static Uri GetSourceUri(IOwinRequest request)
        {
            string origin = request.Query["origin"].Trim();

            bool hasFirstParameter = false;
            StringBuilder url = new StringBuilder();
            if (!origin.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                url.Append("http://");
            }

            url.Append(origin);
            if (origin.Contains("?"))
            {
                hasFirstParameter = true;
            }

            foreach (var queryParam in request.Query)
            {
                if (!queryParam.Key.Equals("origin", StringComparison.OrdinalIgnoreCase))
                {
                    string value = request.Query[queryParam.Key].Trim();

                    if (hasFirstParameter)
                    {
                        url.Append("&");
                    }
                    else
                    {
                        url.Append("?");
                        hasFirstParameter = true;
                    }

                    url.Append(queryParam).Append("=").Append(value);
                }
            }

            return new Uri(url.ToString());
        }
    }
}
