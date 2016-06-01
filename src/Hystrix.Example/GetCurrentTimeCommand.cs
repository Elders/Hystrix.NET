namespace Hystrix.Example
{
    using Netflix.Hystrix;
    using System;
    using System.Threading;
    public class GetCurrentTimeCommand : HystrixCommand<long>
    {
        private static long currentTimeCache;

        public GetCurrentTimeCommand()
            : base(HystrixCommandSetter.WithGroupKey("TimeGroup")
                .AndCommandKey("GetCurrentTime")
                .AndCommandPropertiesDefaults(
                    new HystrixCommandPropertiesSetter()
                    .WithExecutionIsolationThreadTimeout(TimeSpan.FromSeconds(1.0))
                    .WithExecutionIsolationStrategy(ExecutionIsolationStrategy.Semaphore)
                    .WithExecutionIsolationThreadInterruptOnTimeout(true)))
        {
        }

        protected override long Run()
        {
            var random = new Random().Next(1000);

            if (random % 3 == 0)
                throw new Exception();
            if (random % 226 == 0)
                Thread.Sleep(random * 20);
            else
                Thread.Sleep(random);

            return random;

            //using (WebClient wc = new WebClient())
            //{
            //    string content = wc.DownloadString("http://tycho.usno.navy.mil/cgi-bin/time.pl");
            //    XDocument document = XDocument.Parse(content);
            //    currentTimeCache = long.Parse(document.Element("usno").Element("t").Value);
            //    return currentTimeCache;
            //}
        }

        protected override long GetFallback()
        {
            return currentTimeCache;
        }
    }
}
