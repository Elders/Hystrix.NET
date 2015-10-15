namespace Netflix.Hystrix
{
    using Netflix.Hystrix.Util;

    internal class RealCollapserTimer : ICollapserTimer
    {
        public TimerReference AddListener(ITimerListener collapseTask)
        {
            return HystrixTimer.Instance.AddTimerListener(collapseTask);
        }
    }
}
