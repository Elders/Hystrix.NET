namespace Hystrix.Test.Util
{
    using System;
    using System.Threading;
    using Java.Util.Concurrent.Atomic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Netflix.Hystrix;
    using Netflix.Hystrix.Util;

    [TestClass]
    public class HystrixTimerTest
    {
        public class PlayListener : ITimerListener
        {
            private int counter = 0;
            public int Counter { get { return this.Counter; } }

            public void Tick()
            {
                counter++;
            }

            public int IntervalTimeInMilliseconds
            {
                get { return 10; }
            }
        }
        private class TestListener : ITimerListener
        {

            private readonly int interval;
            private AtomicInteger tickCount = new AtomicInteger();
            public AtomicInteger TickCount { get { return this.tickCount; } }

            public TestListener(int interval, String value)
            {
                this.interval = interval;
            }

            public void Tick()
            {
                tickCount.IncrementAndGet();
            }

            public int IntervalTimeInMilliseconds
            {
                get { return interval; }
            }

        }

        [TestMethod]
        public void Timer_SingleCommandSingleInterval()
        {
            HystrixTimer timer = HystrixTimer.Instance;
            TestListener l1 = new TestListener(50, "A");
            timer.AddTimerListener(l1);

            TestListener l2 = new TestListener(50, "B");
            timer.AddTimerListener(l2);

            try
            {
                Thread.Sleep(500);
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.ToString());
            }

            // we should have 7 or more 50ms ticks within 500ms
            Console.WriteLine("l1 ticks: " + l1.TickCount.Value);
            Console.WriteLine("l2 ticks: " + l2.TickCount.Value);
            Assert.IsTrue(l1.TickCount.Value > 7);
            Assert.IsTrue(l2.TickCount.Value > 7);
        }

        [TestMethod]
        public void Timer_SingleCommandMultipleIntervals()
        {
            HystrixTimer timer = HystrixTimer.Instance;
            TestListener l1 = new TestListener(100, "A");
            timer.AddTimerListener(l1);

            TestListener l2 = new TestListener(10, "B");
            timer.AddTimerListener(l2);

            TestListener l3 = new TestListener(25, "C");
            timer.AddTimerListener(l3);

            try
            {
                Thread.Sleep(500);
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.ToString());
            }

            // we should have 3 or more 100ms ticks within 500ms
            Console.WriteLine("l1 ticks: " + l1.TickCount.Value);
            Assert.IsTrue(l1.TickCount.Value >= 3);
            // but it can't be more than 6
            Assert.IsTrue(l1.TickCount.Value < 6);

            // we should have 30 or more 10ms ticks within 500ms
            Console.WriteLine("l2 ticks: " + l2.TickCount);
            Assert.IsTrue(l2.TickCount.Value > 28);
            Assert.IsTrue(l2.TickCount.Value < 550);

            // we should have 15-20 25ms ticks within 500ms
            Console.WriteLine("l3 ticks: " + l3.TickCount);
            Assert.IsTrue(l3.TickCount.Value > 12);
            Assert.IsTrue(l3.TickCount.Value < 25);
        }

        [TestMethod]
        public void Timer_SingleCommandRemoveListener()
        {
            HystrixTimer timer = HystrixTimer.Instance;
            TestListener l1 = new TestListener(50, "A");
            timer.AddTimerListener(l1);

            TestListener l2 = new TestListener(50, "B");
            TimerReference l2ref = timer.AddTimerListener(l2);

            try
            {
                Thread.Sleep(500);
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.ToString());
            }

            // we should have 7 or more 50ms ticks within 500ms
            Console.WriteLine("l1 ticks: " + l1.TickCount);
            Console.WriteLine("l2 ticks: " + l2.TickCount);
            Assert.IsTrue(l1.TickCount.Value > 7);
            Assert.IsTrue(l2.TickCount.Value > 7);

            // remove l2
            l2ref.Clear();

            // reset counts
            l1.TickCount.Value = 0;
            l2.TickCount.Value = 0;

            // wait for time to pass again
            try
            {
                Thread.Sleep(500);
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.ToString());
            }

            // we should have 7 or more 50ms ticks within 500ms
            Console.WriteLine("l1 ticks: " + l1.TickCount);
            Console.WriteLine("l2 ticks: " + l2.TickCount);
            // l1 should continue ticking
            Assert.IsTrue(l1.TickCount.Value > 7);
            // we should have no ticks on l2 because we removed it
            Console.WriteLine("tickCount.get(): " + l2.TickCount + " on l2: " + l2);
            Assert.AreEqual(0, l2.TickCount);
        }

        public static void main(String[] args)
        {
            PlayListener l1 = new PlayListener();
            PlayListener l2 = new PlayListener();
            PlayListener l3 = new PlayListener();
            PlayListener l4 = new PlayListener();
            PlayListener l5 = new PlayListener();

            TimerReference reference = HystrixTimer.Instance.AddTimerListener(l1);
            HystrixTimer.Instance.AddTimerListener(l2);
            HystrixTimer.Instance.AddTimerListener(l3);

            HystrixTimer.Instance.AddTimerListener(l4);

            try
            {
                Thread.Sleep(5000);
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.ToString());
            }

            reference.Clear();
            HystrixTimer.Instance.AddTimerListener(l5);

            try
            {
                Thread.Sleep(10000);
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("counter: " + l1.Counter);
            Console.WriteLine("counter: " + l2.Counter);
            Console.WriteLine("counter: " + l3.Counter);
            Console.WriteLine("counter: " + l4.Counter);
            Console.WriteLine("counter: " + l5.Counter);
        }
    }
}
