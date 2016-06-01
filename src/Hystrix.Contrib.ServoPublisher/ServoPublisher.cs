using Netflix.Hystrix;
using Netflix.Hystrix.CircuitBreaker;
using Netflix.Hystrix.Strategy.Metrics;
using Netflix.Hystrix.ThreadPool;
using Servo.NET.Atlas;

namespace Hystrix.Contrib.ServoPublisher
{
    public class ServoPublisher : IHystrixMetricsPublisher
    {
        CommandMetricObserver observer;

        public ServoPublisher()
        {
            observer = new CommandMetricObserver();
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
