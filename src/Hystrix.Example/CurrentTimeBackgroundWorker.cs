namespace Hystrix.Example
{
    using System;
    using System.Globalization;
    using System.Threading;
    using Hystrix.MetricsEventStream;
    using slf4net;

    internal class CurrentTimeBackgroundWorker : StoppableBackgroundWorker
    {
        private const string ThreadName = "Hystrix-Example-Worker-{0}";

        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(CurrentTimeBackgroundWorker));

        private static int nextId = 1;

        public CurrentTimeBackgroundWorker()
            : base(string.Format(CultureInfo.InvariantCulture, ThreadName, nextId++))
        {
        }

        protected override void DoWork()
        {
            while (true)
            {
                bool shouldStop = this.SleepAndGetShouldStop(TimeSpan.FromSeconds(1.0));
                if (shouldStop)
                {
                    break;
                }

                GetCurrentTimeCommand command = new GetCurrentTimeCommand();
                long currentTime = command.Execute();
                Logger.Trace(string.Format(CultureInfo.InvariantCulture, "The current time is {0}.", currentTime));
            }
        }
    }
}
