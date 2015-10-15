namespace Hystrix.Test.Util
{
    using Netflix.Hystrix.Util;

    internal class MockedTime : ITime
    {
        private long time = 0;

        public long GetCurrentTimeInMillis()
        {
            return this.time;
        }

        public void Increment(int millis)
        {
            this.time += millis;
        }
    }
}
