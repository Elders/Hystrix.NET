// Copyright 2013 Loránd Biró
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Hystrix.MetricsEventStream
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Net;
    using slf4net;

    /// <summary>
    /// Listens on a specified url and port for HTTP requests, creates a <see cref="HystrixMetricsStreamer"/> for every
    /// connections, which will stream the metrics data obtained from a <see cref="HystrixMetricsSampler"/> instance.
    /// Server can be started using the <see cref="Start"/> method, and can be stopped using the <see cref="Stop"/> method.
    /// </summary>
    /// <remarks>
    /// The server uses <see cref="HttpListener"/>, which requires permission to the specified url namespace.
    /// More information can be found on <see href="http://msdn.microsoft.com/en-us/library/ms733768.aspx"/>.
    /// </remarks>
    public class HystrixMetricsStreamServer : StoppableBackgroundWorker
    {
        /// <summary>
        /// The name of the background thread.
        /// </summary>
        private const string ThreadName = "Hystrix-MetricsEventStream-Listener";

        /// <summary>
        /// The logger instance for this class.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(HystrixMetricsStreamServer));

        /// <summary>
        /// The single sampler instance used by all the streamer.
        /// </summary>
        private HystrixMetricsSampler sampler;

        /// <summary>
        /// The <see cref="HttpListener"/> instance used to receive incoming connections.
        /// </summary>
        private HttpListener listener;

        /// <summary>
        /// The streamer instances for the current connections.
        /// </summary>
        private List<HystrixMetricsStreamer> streamers = new List<HystrixMetricsStreamer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixMetricsStreamServer"/> class.
        /// </summary>
        /// <param name="httpListenerPrefix">The http listener prefix for listening incoming connections.
        /// For examples 'http://+:8888/hystrix/' which accepts connections on any domain on the port 8888 for the url '/hystrix/'.</param>
        /// <param name="maxConcurrentConnections">The maximum number of concurrent connections. New clients will be refused if
        /// the number of concurrent connections reach this maximum.</param>
        /// <param name="sampleInterval">The interval between sampling the Hystrix metrics.
        /// Passed through to the <see cref="HystrixMetricsSampler"/></param>
        public HystrixMetricsStreamServer(string httpListenerPrefix, int maxConcurrentConnections, TimeSpan sampleInterval)
            : base(ThreadName)
        {
            if (string.IsNullOrEmpty(httpListenerPrefix))
            {
                throw new ArgumentNullException("httpListenerPrefix");
            }

            if (maxConcurrentConnections <= 0)
            {
                throw new ArgumentException("Maximum number of concurrent connections must be a positive number.", "maxConcurrentConnections");
            }

            this.HttpListenerPrefix = httpListenerPrefix;
            this.MaxConcurrentConnections = maxConcurrentConnections;
            this.SampleInterval = sampleInterval;

            this.sampler = new HystrixMetricsSampler(sampleInterval);
        }

        /// <summary>
        /// Gets the http listener prefix used to listen incoming connections.
        /// </summary>
        public string HttpListenerPrefix { get; private set; }

        /// <summary>
        /// Gets the interval between sampling metrics data.
        /// </summary>
        public TimeSpan SampleInterval { get; private set; }

        /// <summary>
        /// Gets the maximum number of connections.
        /// </summary>
        public int MaxConcurrentConnections { get; private set; }

        /// <summary>
        /// Gets the current number of connections.
        /// </summary>
        public int CurrentConcurrentConnections
        {
            get
            {
                lock (this.streamers)
                {
                    return this.streamers.Count;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnStarting()
        {
            try
            {
                Logger.Info(CultureInfo.InvariantCulture, "Starting metrics stream server on '{0}'.", this.HttpListenerPrefix);
                this.listener = new HttpListener();
                this.listener.Prefixes.Add(this.HttpListenerPrefix);
                this.listener.Start();

                this.sampler = new HystrixMetricsSampler(this.SampleInterval);
            }
            catch (Exception e)
            {
                if (this.listener != null)
                {
                    this.listener.Stop();
                }

                Logger.Error(e, "Failed to start stream server.");

                throw;
            }

            base.OnStarting();
        }

        /// <inheritdoc/>
        protected override void OnStopping()
        {
            this.sampler.Stop();
            this.listener.Stop();
            base.OnStopping();
        }

        /// <inheritdoc/>
        protected override void DoWork()
        {
            try
            {
                while (!this.IsStopping)
                {
                    HttpListenerContext context = this.listener.GetContext();
                    lock (this.streamers)
                    {
                        if (this.streamers.Count >= this.MaxConcurrentConnections)
                        {
                            RefuseConnection(context, "Max concurrent connections reached: " + this.MaxConcurrentConnections);
                            Logger.Warn("Max concurrent connections reached, refusing client request.");
                        }
                        else
                        {
                            if (this.streamers.Count == 0)
                            {
                                this.sampler.Start();
                            }

                            this.CreateAndStartStreamer(context);
                        }
                    }
                }
            }
            finally
            {
                this.Shutdown();
            }
        }

        /// <summary>
        /// Sends back an error message and closes the response stream.
        /// </summary>
        /// <param name="context">The context to work on.</param>
        /// <param name="message">The message containing the reason of refuse.</param>
        private static void RefuseConnection(HttpListenerContext context, string message)
        {
            context.Response.StatusCode = 503;
            context.Response.StatusDescription = message;
            context.Response.Close();
        }

        /// <summary>
        /// Creates and starts a new <see cref="HystrixMetricsStreamer"/> instance.
        /// </summary>
        /// <param name="context">The listener context passed through to the new streamer.</param>
        private void CreateAndStartStreamer(HttpListenerContext context)
        {
            HystrixMetricsStreamer streamer = null;
            try
            {
                streamer = new HystrixMetricsStreamer(this.sampler, context);
                streamer.Stopped += this.Streamer_Stopped;
                streamer.Start();
                this.streamers.Add(streamer);
            }
            catch
            {
                if (streamer != null)
                {
                    streamer.Dispose();
                }

                throw;
            }
        }

        /// <summary>
        /// The shutdown procedure, which stops the underlying listener, sampler and streamer instances.
        /// It's called from the end of <see cref="DoWork"/>.
        /// </summary>
        private void Shutdown()
        {
            HystrixMetricsStreamer[] streamerArray;
            lock (this.streamers)
            {
                streamerArray = this.streamers.ToArray();
            }

            this.sampler.Stop();
            this.listener.Stop();
            foreach (HystrixMetricsStreamer streamer in streamerArray)
            {
                streamer.Stop();
            }

            this.sampler.AwaitTermination();
            foreach (HystrixMetricsStreamer streamer in streamerArray)
            {
                streamer.AwaitTermination();
            }
        }

        /// <summary>
        /// Handles the stopped streamers, disposes and removes them from the list of current steamers.
        /// </summary>
        /// <param name="sender">The streamer which raised this event.</param>
        /// <param name="e">Generic event arguments, not used.</param>
        private void Streamer_Stopped(object sender, AsyncCompletedEventArgs e)
        {
            lock (this.streamers)
            {
                HystrixMetricsStreamer streamer = (HystrixMetricsStreamer)sender;
                streamer.Stopped -= this.Streamer_Stopped;
                streamer.Dispose();
                this.streamers.Remove(streamer);
                if (this.streamers.Count == 0)
                {
                    this.sampler.Stop();
                }
            }
        }
    }
}
