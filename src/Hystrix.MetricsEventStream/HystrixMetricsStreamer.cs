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

namespace Hystrix.MetricsEventStream
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using slf4net;

    /// <summary>
    /// A steamer which is created for every connected client. The streamer will listen the sampler and stream the JSON formatted
    /// metrics data.
    /// </summary>
    internal class HystrixMetricsStreamer : StoppableBackgroundWorker
    {
        /// <summary>
        /// The name format for the background thread.
        /// </summary>
        private const string ThreadNameFormat = "Hystrix-MetricsEventStream-Streamer-{0}";

        /// <summary>
        /// The metrics data queue size limit. The metrics data will thrown away if the queue exceeds this limit.
        /// </summary>
        private const int QueueSizeWarningLimit = 1000;

        /// <summary>
        /// The default time interval between sending new metrics data.
        /// </summary>
        private static readonly TimeSpan DefaultSendInterval = TimeSpan.FromSeconds(1.0);

        /// <summary>
        /// The logger instance for this type.
        /// </summary>
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(HystrixMetricsStreamer));

        /// <summary>
        /// Streamer threads are uniquely named using this id.
        /// </summary>
        private static int nextId = 1;

        /// <summary>
        /// The context which contains the response instance used to deliver the metrics data.
        /// </summary>
        private HttpListenerContext context;

        /// <summary>
        /// Metrics data received from the sampler is stored in this queue before streaming to the client.
        /// </summary>
        private Queue<string> metricsDataQueue = new Queue<string>();

        /// <summary>
        /// The global sampler instance got from the parent <see cref="HystrixMetricsStreamServer"/>.
        /// </summary>
        private HystrixMetricsSampler sampler;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixMetricsStreamer"/> class.
        /// </summary>
        /// <param name="sampler">The sampler to use to get metrics data.</param>
        /// <param name="context">The context of the client connection.</param>
        public HystrixMetricsStreamer(HystrixMetricsSampler sampler, HttpListenerContext context)
            : base(string.Format(CultureInfo.InvariantCulture, ThreadNameFormat, nextId))
        {
            if (sampler == null)
            {
                throw new ArgumentNullException("sampler");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            this.sampler = sampler;
            this.context = context;
            this.Id = nextId++;
        }

        /// <summary>
        /// Gets the unique identifier of this streamer.
        /// </summary>
        public int Id { get; private set; }

        /// <inheritdoc />
        protected override void DoWork()
        {
            this.sampler.SampleDataAvailable += this.Sampler_SampleDataAvailable;

            try
            {
                this.context.Response.AppendHeader("Content-Type", "text/event-stream;charset=UTF-8");
                this.context.Response.AppendHeader("Cache-Control", "no-cache, no-store, max-age=0, must-revalidate");
                this.context.Response.AppendHeader("Pragma", "no-cache");

                TimeSpan sendInterval = this.GetSendInterval();
                using (StreamWriter outputWriter = new StreamWriter(this.context.Response.OutputStream))
                {
                    while (true)
                    {
                        bool shouldStop = this.SleepAndGetShouldStop(sendInterval);
                        if (shouldStop)
                        {
                            break;
                        }

                        string[] metricsData;
                        lock (this.metricsDataQueue)
                        {
                            metricsData = this.metricsDataQueue.ToArray();
                            this.metricsDataQueue.Clear();
                        }

                        if (metricsData.Length == 0)
                        {
                            outputWriter.WriteLine("ping: \n");
                        }
                        else
                        {
                            foreach (string json in metricsData)
                            {
                                outputWriter.WriteLine("data: {0}\n", json);
                            }
                        }

                        outputWriter.Flush();
                    }
                }
            }
            catch (HttpListenerException)
            {
                Logger.Info(CultureInfo.InvariantCulture, "Streaming connection #{0} closed by client.", this.Id);
            }
            finally
            {
                this.context.Response.Close();

                this.sampler.SampleDataAvailable -= this.Sampler_SampleDataAvailable;
            }
        }

        /// <summary>
        /// Extracts the sending time interval from the HTTP request.
        /// </summary>
        /// <returns>The time interval between sending new metrics data.</returns>
        private TimeSpan GetSendInterval()
        {
            TimeSpan sendInterval = DefaultSendInterval;
            if (this.context.Request.QueryString["delay"] != null)
            {
                int streamDelayInMilliseconds = 0;
                if (int.TryParse(this.context.Request.QueryString["delay"], out streamDelayInMilliseconds))
                {
                    sendInterval = TimeSpan.FromMilliseconds(streamDelayInMilliseconds);
                }
                else
                {
                    Logger.Warn(CultureInfo.InvariantCulture, "Invalid delay parameter in request: '{0}'", streamDelayInMilliseconds);
                }
            }

            return sendInterval;
        }

        /// <summary>
        /// Receives the metrics data from the sampler and puts into the data queue.
        /// </summary>
        /// <param name="sender">The sampler which produced the metrics data.</param>
        /// <param name="e">Event arguments containing the JSON formatted metrics data.</param>
        private void Sampler_SampleDataAvailable(object sender, SampleDataAvailableEventArgs e)
        {
            lock (this.metricsDataQueue)
            {
                if (this.metricsDataQueue.Count + e.Data.Count() > QueueSizeWarningLimit)
                {
                    Logger.Warn(CultureInfo.InvariantCulture, "Streamer #{0} data queue is full, metrics thrown away.", this.Id);
                    return;
                }

                foreach (string data in e.Data)
                {
                    this.metricsDataQueue.Enqueue(data);
                }
            }
        }
    }
}
