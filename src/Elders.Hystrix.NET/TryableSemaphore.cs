namespace Elders.Hystrix.NET
{
    using Java.Util.Concurrent.Atomic;

    /// <summary>
    /// Semaphore that only supports TryAcquire and never blocks and that supports a dynamic permit count.
    /// </summary>
    /// <remarks>
    /// Using AtomicInteger increment/decrement instead of java.util.concurrent.Semaphore since we don't need blocking and need a custom implementation to get the dynamic permit count and since
    /// AtomicInteger achieves the same behavior and performance without the more complex implementation of the actual Semaphore class using AbstractQueueSynchronizer.
    /// </remarks>
    internal class TryableSemaphore
    {
        private readonly IHystrixProperty<int> numberOfPermits;
        private readonly AtomicInteger count = new AtomicInteger(0);

        public IHystrixProperty<int> NumberOfPermits { get { return this.numberOfPermits; } }

        public TryableSemaphore(IHystrixProperty<int> numberOfPermits)
        {
            this.numberOfPermits = numberOfPermits;
        }

        /// <summary>
        /// Tries to acquire the semaphore.
        /// </summary>
        /// <example>
        /// if (s.TryAcquire())
        /// {
        ///     try
        ///     {
        ///         // do work that is protected by 's'
        ///     }
        ///     finally
        ///     {
        ///         s.Release();
        ///     }
        /// }
        /// </example>
        /// <returns></returns>
        public bool TryAcquire()
        {
            int currentCount = this.count.IncrementAndGet();
            if (currentCount > this.numberOfPermits.Get())
            {
                this.count.DecrementAndGet();
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Releases the acquired semaphore.
        /// </summary>
        /// <example>
        /// if (s.TryAcquire())
        /// {
        ///     try
        ///     {
        ///         // do work that is protected by 's'
        ///     }
        ///     finally
        ///     {
        ///         s.Release();
        ///     }
        /// }
        /// </example>
        public void Release()
        {
            this.count.DecrementAndGet();
        }

        public int GetNumberOfPermitsUsed()
        {
            return this.count.Value;
        }
    }
}
