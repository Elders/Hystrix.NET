namespace Java.Util.Concurrent
{
    /// <summary>
    /// A task that returns a result and may throw an exception. Implementors define a single method with no arguments called Call.
    /// </summary>
    public interface ICallable<T>
    {
        /// <summary>
        /// Computes a result, or throws an exception if unable to do so.
        /// </summary>
        /// <returns>The computed result.</returns>
        T Call();
    }
}
