namespace Elders.Hystrix.NET
{
    using Elders.Hystrix.NET.Util;

    internal interface ICollapserTimer
    {
        TimerReference AddListener(ITimerListener collapseTask);
    }
}
