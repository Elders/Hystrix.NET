namespace Hystrix.Example
{
    using System;
    using System.Net;
    using System.Xml.Linq;
    using Netflix.Hystrix;

    public class GetCurrentTimeCommand : HystrixCommand<long>
    {
        private static long currentTimeCache;

        public GetCurrentTimeCommand()
            : base(HystrixCommandSetter.WithGroupKey("TimeGroup")
                .AndCommandKey("GetCurrentTime")
                .AndCommandPropertiesDefaults(new HystrixCommandPropertiesSetter().WithExecutionIsolationThreadTimeout(TimeSpan.FromSeconds(1.0)).WithExecutionIsolationThreadInterruptOnTimeout(true)))
        {
        }

        protected override long Run()
        {
            using (WebClient wc = new WebClient())
            {
                string content = wc.DownloadString("http://tycho.usno.navy.mil/cgi-bin/time.pl");
                XDocument document = XDocument.Parse(content);
                currentTimeCache = long.Parse(document.Element("usno").Element("t").Value);
                return currentTimeCache;
            }
        }

        protected override long GetFallback()
        {
            return currentTimeCache;
        }
    }
}
