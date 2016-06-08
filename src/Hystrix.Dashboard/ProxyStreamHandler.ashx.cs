// Copyright 2013 Loránd Biró
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Hystrix.Dashboard
{
    using Logging;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;

    /// <summary>
    /// An HTTP handler for proxy streaming. This is necessary because some browsers don't support cross-site streaming.
    /// Continuously transfers the data from the source uri to the client.
    /// The source must be specified in the 'origin' query string parameter.
    /// </summary>
    public class ProxyStreamHandler : IHttpHandler
    {
        /// <summary>
        /// A logger instance for producing log messages.
        /// </summary>
        private static readonly ILog Logger = LogProvider.GetLogger(typeof(ProxyStreamHandler));

        /// <summary>
        /// Gets a value indicating whether to reuse the <see cref="ProxyStreamHandler"/> instances or not.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Continuously transfers the data from the source uri to the client.
        /// The source must be specified in the 'origin' query string parameter.
        /// </summary>
        /// <param name="context">The client request.</param>
        public void ProcessRequest(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (context.Request.QueryString["origin"] == null)
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
                        context.Response.AppendHeader("Content-Type", "text/event-stream;charset=UTF-8");
                        context.Response.AppendHeader("Cache-Control", "no-cache, no-store, max-age=0, must-revalidate");
                        context.Response.AppendHeader("Pragma", "no-cache");

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
        private static void StreamSourceToClient(HttpWebResponse sourceResponse, HttpResponse clientResponse)
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
                            clientResponse.Flush();
                        }
                        catch (HttpException)
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
        private static Uri GetSourceUri(HttpRequest request)
        {
            string origin = request.Params["origin"].Trim();

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

            foreach (string key in request.QueryString.Keys)
            {
                if (!key.Equals("origin", StringComparison.OrdinalIgnoreCase))
                {
                    string value = request.QueryString[key].Trim();

                    if (hasFirstParameter)
                    {
                        url.Append("&");
                    }
                    else
                    {
                        url.Append("?");
                        hasFirstParameter = true;
                    }

                    url.Append(key).Append("=").Append(value);
                }
            }

            return new Uri(url.ToString());
        }
    }
}