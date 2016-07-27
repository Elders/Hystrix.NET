namespace Elders.Hystrix.NET
{
    using Elders.Hystrix.NET.Util;

    internal class RealCollapserTimer : ICollapserTimer
    {
        public TimerReference AddListener(ITimerListener collapseTask)
        {
            return HystrixTimer.Instance.AddTimerListener(collapseTask);
        }
    }
}
