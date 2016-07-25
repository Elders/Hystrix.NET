using Hystrix.NET.Contrib.ServoPublisher;
using Netflix.Hystrix;
using Netflix.Hystrix.CircuitBreaker;
using Netflix.Hystrix.Strategy.Metrics;
using Netflix.Hystrix.ThreadPool;
using Servo.NET.Atlas;

namespace Hystrix.Example
{
    public class AtlasPublisher : IHystrixMetricsPublisher
    {
        CommandMetricObserver observer;

        public AtlasPublisher()
        {
            var atlasCfg = new AtlasConfig("http://10.0.75.2", 7101);
            observer = new CommandMetricObserver(atlasCfg);
            observer.Run();
        }

        public IHystrixMetricsPublisherCommand GetMetricsPublisherForCommand(HystrixCommandKey commandKey, HystrixCommandGroupKey commandGroupKey, HystrixCommandMetrics metrics, IHystrixCircuitBreaker circuitBreaker, IHystrixCommandProperties properties)
        {
            return new HystrixServoMetricsPublisherCommand(commandKey, commandGroupKey, metrics, circuitBreaker, properties);
        }

        public IHystrixMetricsPublisherThreadPool GetMetricsPublisherForThreadPool(HystrixThreadPoolKey threadPoolKey, HystrixThreadPoolMetrics metrics, IHystrixThreadPoolProperties properties)
        {
            return new Dummy1();
        }

        public void Dispose()
        {
            observer?.Stop();
            observer = null;
        }
    }

    public class Dummy1 : IHystrixMetricsPublisherThreadPool
    {
        public void Initialize()
        {

        }
    }
}
