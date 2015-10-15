namespace Netflix.Hystrix
{
    using Netflix.Hystrix.Util;

    internal interface ICollapserTimer
    {
        TimerReference AddListener(ITimerListener collapseTask);
    }
}
