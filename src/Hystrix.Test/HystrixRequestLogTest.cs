namespace Hystrix.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Netflix.Hystrix;
    using Netflix.Hystrix.Strategy.Concurrency;

    [TestClass]
    public class HystrixRequestLogTest
    {
        [TestMethod]
        public void RequestLog_Success()
        {
            HystrixRequestContext context = HystrixRequestContext.InitializeContext();
            try
            {
                new TestCommand("A", false, true).Execute();
                String log = HystrixRequestLog.GetCurrentRequest().GetExecutedCommandsAsString();
                // strip the actual count so we can compare reliably
                log = Regex.Replace(log, "\\[\\d*", "[");
                Assert.AreEqual("TestCommand[Success][ms]", log);
            }
            finally
            {
                context.Shutdown();
            }
        }

        [TestMethod]
        public void RequestLog_SuccessFromCache()
        {
            HystrixRequestContext context = HystrixRequestContext.InitializeContext();
            try
            {
                // 1 success
                new TestCommand("A", false, true).Execute();
                // 4 success from cache
                new TestCommand("A", false, true).Execute();
                new TestCommand("A", false, true).Execute();
                new TestCommand("A", false, true).Execute();
                new TestCommand("A", false, true).Execute();
                String log = HystrixRequestLog.GetCurrentRequest().GetExecutedCommandsAsString();
                // strip the actual count so we can compare reliably
                log = Regex.Replace(log, "\\[\\d*", "[");
                Assert.AreEqual("TestCommand[Success][ms], TestCommand[Success, ResponseFromCache][ms]x4", log);
            }
            finally
            {
                context.Shutdown();
            }
        }

        [TestMethod]
        public void RequestLog_FailWithFallbackSuccess()
        {
            HystrixRequestContext context = HystrixRequestContext.InitializeContext();
            try
            {
                // 1 failure
                new TestCommand("A", true, false).Execute();
                // 4 failures from cache
                new TestCommand("A", true, false).Execute();
                new TestCommand("A", true, false).Execute();
                new TestCommand("A", true, false).Execute();
                new TestCommand("A", true, false).Execute();
                String log = HystrixRequestLog.GetCurrentRequest().GetExecutedCommandsAsString();
                // strip the actual count so we can compare reliably
                log = Regex.Replace(log, "\\[\\d*", "[");
                Assert.AreEqual("TestCommand[Failure, FallbackSuccess][ms], TestCommand[Failure, FallbackSuccess, ResponseFromCache][ms]x4", log);
            }
            finally
            {
                context.Shutdown();
            }
        }

        [TestMethod]
        public void RequestLog_FailWithFallbackFailure()
        {
            HystrixRequestContext context = HystrixRequestContext.InitializeContext();
            try
            {
                // 1 failure
                try
                {
                    new TestCommand("A", true, true).Execute();
                }
                catch (Exception)
                {
                }
                // 1 failure from cache
                try
                {
                    new TestCommand("A", true, true).Execute();
                }
                catch (Exception)
                {
                }
                String log = HystrixRequestLog.GetCurrentRequest().GetExecutedCommandsAsString();
                // strip the actual count so we can compare reliably
                log = Regex.Replace(log, "\\[\\d*", "[");
                Assert.AreEqual("TestCommand[Failure, FallbackFailure][ms], TestCommand[Failure, FallbackFailure, ResponseFromCache][ms]", log);
            }
            finally
            {
                context.Shutdown();
            }
        }

        [TestMethod]
        public void RequestLog_MultipleCommands()
        {

            HystrixRequestContext context = HystrixRequestContext.InitializeContext();
            try
            {

                // 1 success
                new TestCommand("GetData", "A", false, false).Execute();

                // 1 success
                new TestCommand("PutData", "B", false, false).Execute();

                // 1 success
                new TestCommand("GetValues", "C", false, false).Execute();

                // 1 success from cache
                new TestCommand("GetValues", "C", false, false).Execute();

                // 1 failure
                try
                {
                    new TestCommand("A", true, true).Execute();
                }
                catch (Exception)
                {
                }
                // 1 failure from cache
                try
                {
                    new TestCommand("A", true, true).Execute();
                }
                catch (Exception)
                {
                }
                String log = HystrixRequestLog.GetCurrentRequest().GetExecutedCommandsAsString();
                // strip the actual count so we can compare reliably
                log = Regex.Replace(log, "\\[\\d*", "[");
                Assert.AreEqual("GetData[Success][ms], PutData[Success][ms], GetValues[Success][ms], GetValues[Success, ResponseFromCache][ms], TestCommand[Failure, FallbackFailure][ms], TestCommand[Failure, FallbackFailure, ResponseFromCache][ms]", log);
            }
            finally
            {
                context.Shutdown();
            }

        }

        [TestMethod]
        public void RequestLog_MaxLimit()
        {
            HystrixRequestContext context = HystrixRequestContext.InitializeContext();
            try
            {
                for (int i = 0; i < HystrixRequestLog.MaxStorage; i++)
                {
                    new TestCommand("A", false, true).Execute();
                }
                // then execute again some more
                for (int i = 0; i < 10; i++)
                {
                    new TestCommand("A", false, true).Execute();
                }

                Assert.AreEqual(HystrixRequestLog.MaxStorage, HystrixRequestLog.GetCurrentRequest().ExecutedCommands.Count());
            }
            finally
            {
                context.Shutdown();
            }
        }

        private class TestCommand : HystrixCommand<string>
        {
            private readonly string value;
            private readonly bool fail;
            private readonly bool failOnFallback;

            public TestCommand(string commandName, string value, bool fail, bool failOnFallback)
                : base(HystrixCommandSetter
                    .WithGroupKey("RequestLogTestCommand")
                    .AndCommandKey(commandName))
            {
                this.value = value;
                this.fail = fail;
                this.failOnFallback = failOnFallback;
            }
            public TestCommand(string value, bool fail, bool failOnFallback)
                : base(HystrixCommandSetter
                    .WithGroupKey("RequestLogTestCommand"))
            {
                this.value = value;
                this.fail = fail;
                this.failOnFallback = failOnFallback;
            }

            protected override string Run()
            {
                if (fail)
                {
                    throw new Exception("forced failure");
                }
                else
                {
                    return value;
                }
            }
            protected override string GetFallback()
            {
                if (failOnFallback)
                {
                    throw new Exception("forced fallback failure");
                }
                else
                {
                    return value + "-fallback";
                }
            }
            protected override string GetCacheKey()
            {
                return value;
            }
        }
    }
}
