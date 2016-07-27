namespace Elders.Hystrix.NET
{
    /// <summary>
    /// Stores summarized health metrics about HystrixCommands.
    /// </summary>
    public class HealthCounts
    {
        private readonly long totalCount;
        private readonly long errorCount;
        private readonly int errorPercentage;

        /// <summary>
        /// The total number of requests made by this command.
        /// </summary>
        public long TotalRequests { get { return this.totalCount; } }

        /// <summary>
        /// The total number of errors made by this command.
        /// This includes Failure, Timeout, ThreadPoolRejected, ShortCircuited and SemaphoreRejected <see cref="HystrixRollingNumberEvent"/> events.
        /// </summary>
        public long ErrorCount { get { return this.errorCount; } }

        /// <summary>
        /// The ratio of total requests and error counts in percents.
        /// </summary>
        public int ErrorPercentage { get { return this.errorPercentage; } }

        /// <summary>
        /// Initializes a new instance of HealthCounts.
        /// </summary>
        /// <param name="total">The total number of requests made by this command.</param>
        /// <param name="error">The total number of errors made by this command.</param>
        public HealthCounts(long total, long error)
        {
            this.totalCount = total;
            this.errorCount = error;

            if (total > 0)
            {
                this.errorPercentage = (int)((double)error / total * 100);
            }
            else
            {
                this.errorPercentage = 0;
            }
        }
    }
}
