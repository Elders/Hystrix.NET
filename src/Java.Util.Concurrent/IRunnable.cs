namespace Java.Util.Concurrent
{
    /// <summary>
    /// The Runnable interface should be implemented by any class whose instances are intended
    /// to be executed by a thread. The class must define a method of no arguments called run.
    /// </summary>
    public interface IRunnable
    {
        /// <summary>
        /// When an object implementing interface IRunnable is used to create a thread, starting
        /// the thread causes the object's Run method to be called in that separately executing thread.
        /// The general contract of the method Run is that it may take any action whatsoever.
        /// </summary>
        void Run();
    }
}
