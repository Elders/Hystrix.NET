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
    using Netflix.Hystrix;
    using Netflix.Hystrix.CircuitBreaker;
    using Netflix.Hystrix.ThreadPool;
    using Netflix.Hystrix.Util;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using slf4net;

    /// <summary>
    /// Samples Hystrix metrics (<see cref="HystrixCommandMetrics.Instances"/>, <see cref="HystrixThreadPoolMetrics.Instances"/>)
    /// and outputs them as JSON formatted strings. Sampling and processing done in a different thread
    /// which can be started and stopped with the <see cref="Start"/> and <see cref="Stop"/> methods. Receiving the
    /// formatted data is possible through the <see cref="SampleDataAvailable"/> event.
    /// </summary>
    public class HystrixMetricsSampler : StoppableBackgroundWorker
    {
        /// <summary>
        /// The name of the sampler thread.
        /// </summary>
        private const string ThreadName = "Hystrix-MetricsEventStream-Sampler";

        /// <summary>
        /// The date which is used to calculate the current time for JavaScript.
        /// JavaScript measures the time in elapsed milliseconds since 1970.01.01 00:00:00.
        /// </summary>
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// The backing field for the <see cref="SampleInterval"/> property. May be access only through the property.
        /// </summary>
        private TimeSpan sampleInterval;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixMetricsSampler" /> class.
        /// </summary>
        /// <param name="sampleInterval">The time interval between sampling.</param>
        public HystrixMetricsSampler(TimeSpan sampleInterval)
            : base(ThreadName)
        {
            this.SampleInterval = sampleInterval;
        }

        /// <summary>
        /// When the sampler is running, it will repeatedly broadcast the sample data through this event.
        /// You should not do long operations in the event handler, since it would block the operation of
        /// the sampler.
        /// </summary>
        public event EventHandler<SampleDataAvailableEventArgs> SampleDataAvailable;

        /// <summary>
        /// Gets or sets the interval between sampling, it must be a positive time.
        /// </summary>
        public TimeSpan SampleInterval
        {
            get
            {
                return this.sampleInterval;
            }

            set
            {
                if (value.Ticks <= 0)
                {
                    throw new ArgumentException("Sample interval must be greater than zero.");
                }

                this.sampleInterval = value;
            }
        }

        /// <inheritdoc />
        protected override void DoWork()
        {
            List<string> data = new List<string>();
            while (true)
            {
                bool shouldStop = this.SleepAndGetShouldStop(this.SampleInterval);
                if (shouldStop)
                {
                    break;
                }

                foreach (HystrixCommandMetrics commandMetrics in HystrixCommandMetrics.Instances)
                {
                    data.Add(CreateCommandSampleData(commandMetrics));
                }

                foreach (HystrixThreadPoolMetrics threadPoolMetrics in HystrixThreadPoolMetrics.Instances)
                {
                    data.Add(CreateThreadPoolSampleData(threadPoolMetrics));
                }

                EventHandler<SampleDataAvailableEventArgs> handler = this.SampleDataAvailable;
                if (handler != null)
                {
                    handler(this, new SampleDataAvailableEventArgs(data));
                }

                data.Clear();
            }
        }

        /// <summary>
        /// Produces JSON formatted metrics data from an instance of <see cref="HystrixCommandMetrics"/>.
        /// </summary>
        /// <param name="commandMetrics">The metrics of a command.</param>
        /// <returns>JSON formatted metrics data.</returns>
        private static string CreateCommandSampleData(HystrixCommandMetrics commandMetrics)
        {
            IHystrixCircuitBreaker circuitBreaker = HystrixCircuitBreakerFactory.GetInstance(commandMetrics.CommandKey);
            HealthCounts healthCounts = commandMetrics.GetHealthCounts();
            IHystrixCommandProperties commandProperties = commandMetrics.Properties;

            JObject data = new JObject(
                new JProperty("type", "HystrixCommand"),
                new JProperty("name", commandMetrics.CommandKey.Name),
                new JProperty("group", commandMetrics.CommandGroup.Name),
                new JProperty("currentTime", GetCurrentTimeForJavascript()),
                circuitBreaker == null ? new JProperty("isCircuitBreakerOpen", false) : new JProperty("isCircuitBreakerOpen", circuitBreaker.IsOpen()),
                new JProperty("errorPercentage", healthCounts.ErrorPercentage), // health counts
                new JProperty("errorCount", healthCounts.ErrorCount),
                new JProperty("requestCount", healthCounts.TotalRequests),
                new JProperty("rollingCountCollapsedRequests", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.Collapsed)), // rolling counters
                new JProperty("rollingCountExceptionsThrown", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ExceptionThrown)),
                new JProperty("rollingCountFailure", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.Failure)),
                new JProperty("rollingCountFallbackFailure", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.FallbackFailure)),
                new JProperty("rollingCountFallbackRejection", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.FallbackRejection)),
                new JProperty("rollingCountFallbackSuccess", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.FallbackSuccess)),
                new JProperty("rollingCountResponsesFromCache", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ResponseFromCache)),
                new JProperty("rollingCountSemaphoreRejected", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.SemaphoreRejected)),
                new JProperty("rollingCountShortCircuited", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ShortCircuited)),
                new JProperty("rollingCountSuccess", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.Success)),
                new JProperty("rollingCountThreadPoolRejected", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.ThreadPoolRejected)),
                new JProperty("rollingCountTimeout", commandMetrics.GetRollingCount(HystrixRollingNumberEvent.Timeout)),
                new JProperty("currentConcurrentExecutionCount", commandMetrics.CurrentConcurrentExecutionCount),
                new JProperty("latencyExecute_mean", commandMetrics.GetExecutionTimeMean()), // latency percentiles
                new JProperty(
                    "latencyExecute",
                    new JObject(
                        new JProperty("0", commandMetrics.GetExecutionTimePercentile(0)),
                        new JProperty("25", commandMetrics.GetExecutionTimePercentile(25)),
                        new JProperty("50", commandMetrics.GetExecutionTimePercentile(50)),
                        new JProperty("75", commandMetrics.GetExecutionTimePercentile(75)),
                        new JProperty("90", commandMetrics.GetExecutionTimePercentile(90)),
                        new JProperty("95", commandMetrics.GetExecutionTimePercentile(95)),
                        new JProperty("99", commandMetrics.GetExecutionTimePercentile(99)),
                        new JProperty("99.5", commandMetrics.GetExecutionTimePercentile(99.5)),
                        new JProperty("100", commandMetrics.GetExecutionTimePercentile(100)))),
                new JProperty("latencyTotal_mean", commandMetrics.GetTotalTimeMean()),
                new JProperty(
                    "latencyTotal",
                    new JObject(
                        new JProperty("0", commandMetrics.GetTotalTimePercentile(0)),
                        new JProperty("25", commandMetrics.GetTotalTimePercentile(25)),
                        new JProperty("50", commandMetrics.GetTotalTimePercentile(50)),
                        new JProperty("75", commandMetrics.GetTotalTimePercentile(75)),
                        new JProperty("90", commandMetrics.GetTotalTimePercentile(90)),
                        new JProperty("95", commandMetrics.GetTotalTimePercentile(95)),
                        new JProperty("99", commandMetrics.GetTotalTimePercentile(99)),
                        new JProperty("99.5", commandMetrics.GetTotalTimePercentile(99.5)),
                        new JProperty("100", commandMetrics.GetTotalTimePercentile(100)))),
                new JProperty("propertyValue_circuitBreakerRequestVolumeThreshold", commandProperties.CircuitBreakerRequestVolumeThreshold.Get()), // property values for reporting what is actually seen by the command rather than what was set somewhere 
                new JProperty("propertyValue_circuitBreakerSleepWindowInMilliseconds", (long)commandProperties.CircuitBreakerSleepWindow.Get().TotalMilliseconds),
                new JProperty("propertyValue_circuitBreakerErrorThresholdPercentage", commandProperties.CircuitBreakerErrorThresholdPercentage.Get()),
                new JProperty("propertyValue_circuitBreakerForceOpen", commandProperties.CircuitBreakerForceOpen.Get()),
                new JProperty("propertyValue_circuitBreakerForceClosed", commandProperties.CircuitBreakerForceClosed.Get()),
                new JProperty("propertyValue_circuitBreakerEnabled", commandProperties.CircuitBreakerEnabled.Get()),
                new JProperty("propertyValue_executionIsolationStrategy", commandProperties.ExecutionIsolationStrategy.Get()),
                new JProperty("propertyValue_executionIsolationThreadTimeoutInMilliseconds", (long)commandProperties.ExecutionIsolationThreadTimeout.Get().TotalMilliseconds),
                new JProperty("propertyValue_executionIsolationThreadInterruptOnTimeout", commandProperties.ExecutionIsolationThreadInterruptOnTimeout.Get()),
                new JProperty("propertyValue_executionIsolationThreadPoolKeyOverride", commandProperties.ExecutionIsolationThreadPoolKeyOverride.Get()),
                new JProperty("propertyValue_executionIsolationSemaphoreMaxConcurrentRequests", commandProperties.ExecutionIsolationSemaphoreMaxConcurrentRequests.Get()),
                new JProperty("propertyValue_fallbackIsolationSemaphoreMaxConcurrentRequests", commandProperties.FallbackIsolationSemaphoreMaxConcurrentRequests.Get()),
                new JProperty("propertyValue_metricsRollingStatisticalWindowInMilliseconds", commandProperties.MetricsRollingStatisticalWindowInMilliseconds.Get()),
                new JProperty("propertyValue_requestCacheEnabled", commandProperties.RequestCacheEnabled.Get()),
                new JProperty("propertyValue_requestLogEnabled", commandProperties.RequestLogEnabled.Get()),
                new JProperty("reportingHosts", 1));

            return data.ToString(Formatting.None);
        }

        /// <summary>
        /// Produces JSON formatted metrics data from an instance of <see cref="HystrixThreadPoolMetrics"/>.
        /// </summary>
        /// <param name="threadPoolMetrics">The metrics of a thread pool.</param>
        /// <returns>JSON formatted metrics data.</returns>
        private static string CreateThreadPoolSampleData(HystrixThreadPoolMetrics threadPoolMetrics)
        {
            IHystrixThreadPoolProperties properties = threadPoolMetrics.Properties;

            JObject data = new JObject(
                new JProperty("type", "HystrixThreadPool"),
                new JProperty("name", threadPoolMetrics.ThreadPoolKey.Name),
                new JProperty("currentTime", GetCurrentTimeForJavascript()),
                new JProperty("currentActiveCount", threadPoolMetrics.CurrentActiveCount),
                new JProperty("currentCompletedTaskCount", threadPoolMetrics.CurrentCompletedTaskCount),
                new JProperty("currentCorePoolSize", threadPoolMetrics.CurrentCorePoolSize),
                new JProperty("currentLargestPoolSize", threadPoolMetrics.CurrentLargestPoolSize),
                new JProperty("currentMaximumPoolSize", threadPoolMetrics.CurrentMaximumPoolSize),
                new JProperty("currentPoolSize", threadPoolMetrics.CurrentPoolSize),
                new JProperty("currentQueueSize", threadPoolMetrics.CurrentQueueSize),
                new JProperty("currentTaskCount", threadPoolMetrics.CurrentTaskCount),
                new JProperty("rollingCountThreadsExecuted", threadPoolMetrics.RollingCountThreadsExecuted),
                new JProperty("rollingMaxActiveThreads", threadPoolMetrics.RollingMaxActiveThreads),
                new JProperty("propertyValue_queueSizeRejectionThreshold", properties.QueueSizeRejectionThreshold.Get()),
                new JProperty("propertyValue_metricsRollingStatisticalWindowInMilliseconds", properties.MetricsRollingStatisticalWindowInMilliseconds.Get()),
                new JProperty("reportingHosts", 1));

            return data.ToString(Formatting.None);
        }

        /// <summary>
        /// Gets the current time in the format of JavaScript, which is the elapsed 
        /// time since 1970.01.01 00:00:00 in milliseconds.
        /// </summary>
        /// <returns>The current time.</returns>
        private static long GetCurrentTimeForJavascript()
        {
            return (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
        }
    }
}
