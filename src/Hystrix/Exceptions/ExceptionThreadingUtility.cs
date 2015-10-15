namespace Netflix.Hystrix.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Used to capture a stacktrace from one thread and append it to the stacktrace of another
    /// <p>
    /// This is used to make the exceptions thrown from an isolation thread useful, otherwise they see a stacktrace on a thread but never see who was calling it.
    /// </summary>
    public static class ExceptionThreadingUtility
    {
        public static StackTrace GetCallingThreadStack()
        {
            try
            {
                if (callingThreadCache.IsValueCreated)
                {
                    return GetStackTrace(callingThreadCache.Value);
                }
            }
            catch (Exception)
            {
                // ignore ... we don't want to blow up while trying to augment the stacktrace on a real exception
            }
            return null;
        }

        /// <summary>
        /// Allow a parent thread to pass its reference in on a child thread and be remembered so that stacktraces can include the context of who called it.
        /// <p>
        /// ThreadVariable is a Netflix version of ThreadLocal that takes care of cleanup as part of the request/response loop.
        /// </summary>
        private static ThreadLocal<Thread> callingThreadCache = new ThreadLocal<Thread>();

        /// <summary>
        /// http://stackoverflow.com/questions/285031/how-to-get-non-current-threads-stacktrace/14935378#14935378
        /// </summary>
        /// <param name="targetThread"></param>
        /// <returns></returns>
        private static StackTrace GetStackTrace(Thread targetThread)
        {
            ManualResetEventSlim fallbackThreadReady = new ManualResetEventSlim();
            ManualResetEventSlim exitedSafely = new ManualResetEventSlim();
            try
            {
                new Thread(delegate()
                {
                    fallbackThreadReady.Set();
                    if (!exitedSafely.Wait(200))
                    {
                        try
                        {
                            targetThread.Resume();
                        }
                        catch (Exception) {/*Whatever happens, do never stop to resume the main-thread regularly until the main-thread has exited safely.*/}
                    }
                }).Start();
                fallbackThreadReady.Wait();
                //From here, you have about 200ms to get the stack-trace.
                targetThread.Suspend();
                StackTrace trace = null;
                try
                {
                    trace = new StackTrace(targetThread, true);
                }
                catch (ThreadStateException)
                {
                    //failed to get stack trace, since the fallback-thread resumed the thread
                    //possible reasons:
                    //1.) This thread was just too slow
                    //2.) A deadlock ocurred
                    //Automatic retry seems too risky here, so just return null.
                }
                try
                {
                    targetThread.Resume();
                }
                catch (ThreadStateException) {/*Thread is running again already*/}
                return trace;
            }
            finally
            {
                //Just signal the backup-thread to stop.
                exitedSafely.Set();
            }
        }

        // TODO this doesn't get cleaned up after moving away from the Netflix ThreadVariable
        public static void AssignCallingThread(Thread callingThread)
        {
            callingThreadCache.Value = callingThread;
        }
    }
}
