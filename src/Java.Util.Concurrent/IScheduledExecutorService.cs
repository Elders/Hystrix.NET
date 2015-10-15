using System;

namespace Java.Util.Concurrent
{
	/// <summary> 
	/// An <see cref="Spring.Threading.Execution.IExecutorService"/>  that can schedule commands to run after a given
	/// delay, or to execute periodically.
	/// 
	/// <p/> 
	/// The <see cref="Schedule{T}(ICallable{T}, TimeSpan)"/> and
	/// <see cref="Schedule(IRunnable, TimeSpan)"/> 
	/// methods create tasks with various delays
	/// and return a task object that can be used to cancel or check
	/// execution. The <see cref="Spring.Threading.Execution.IScheduledExecutorService.ScheduleAtFixedRate(IRunnable, TimeSpan, TimeSpan)"/> and
	/// <see cref="Spring.Threading.Execution.IScheduledExecutorService.ScheduleWithFixedDelay(IRunnable, TimeSpan, TimeSpan)"/>
	/// methods create and execute tasks
	/// that run periodically until cancelled.
	/// 
	/// <p/> 
	/// Commands submitted using the <see cref="Spring.Threading.IExecutor.Execute(IRunnable)"/>  and
	/// <see cref="M:Spring.Threading.Execution.IExecutorService.Submit"/> methods are scheduled with
	/// a requested delay of zero. Zero and negative delays (but not
    /// periods) are also allowed in <see cref="M:Spring.Threading.Execution.IScheduledExecutorService.Schedule"/> methods, and are
	/// treated as requests for immediate execution.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
    /// <author>Kenneth Xu (.NET)</author>
	public interface IScheduledExecutorService : IExecutorService
	{
		/// <summary> 
		/// Creates and executes a one-shot action that becomes enabled
		/// after the given delay.
		/// </summary>
		/// <param name="command">the task to execute.</param>
		/// <param name="delay">the <see cref="System.TimeSpan"/> from now to delay execution.</param>
		/// <returns> 
		/// a <see cref="IScheduledFuture{T}"/> representing pending completion of the task,
		/// and whose <see cref="IFuture{T}.GetResult()"/> method will return <c>null</c>
		/// upon completion.
		/// </returns>
		IScheduledFuture<Void> Schedule(IRunnable command, TimeSpan delay);

		/// <summary> 
		/// Creates and executes a <see cref="IScheduledFuture{T}"/> that becomes enabled after the
		/// given delay.
		/// </summary>
		/// <param name="callable">the function to execute.</param>
		/// <param name="delay">the <see cref="System.TimeSpan"/> from now to delay execution.</param>
		/// <returns> a <see cref="IScheduledFuture{T}"/> that can be used to extract result or cancel.
		/// </returns>
		IScheduledFuture<T> Schedule<T>(ICallable<T> callable, TimeSpan delay);

		/// <summary> 
		/// Creates and executes a periodic action that becomes enabled first
		/// after the given initial <paramref name="initialDelay"/>, and subsequently with the given
		/// <paramref name="period"/>
		/// </summary>
		/// <remarks>
		/// That is executions will commence after
		/// <paramref name="initialDelay"/>, the <paramref name="initialDelay"/> + <paramref name="period"/>, then
		/// <paramref name="initialDelay"/> + 2 * <paramref name="period"/>, and so on.
		/// <p/>
		/// 
		/// If any execution of the task
		/// encounters an exception, subsequent executions are suppressed.
		/// Otherwise, the task will only terminate via cancellation or
		/// termination of the executor. If any execution of this task 
		/// takes longer than its period, then subsequent executions
		/// may start late, but will <b>NOT</b> concurrently execute.
		/// </remarks>
		/// <param name="command">the task to execute.</param>
		/// <param name="initialDelay">the time to delay first execution.</param>
		/// <param name="period">the period between successive executions.</param>
		/// <returns> a <see cref="IFuture{T}"/> representing pending completion of the task,
		/// and whose <see cref="IFuture{T}.GetResult()"/> method will throw an exception upon
		/// cancellation.
		/// </returns>
		IScheduledFuture<Void> ScheduleAtFixedRate(IRunnable command, TimeSpan initialDelay, TimeSpan period);

		/// <summary> 
		/// Creates and executes a periodic action that becomes enabled first
		/// after the given initial delay, and subsequently with the
		/// given delay between the termination of one execution and the
		/// commencement of the next. 
		/// </summary>
		/// <remarks> 
		/// If any execution of the task
		/// encounters an exception, subsequent executions are suppressed.
		/// Otherwise, the task will only terminate via cancellation or
		/// termination of the executor.
		/// </remarks>
		/// <param name="command">the task to execute.</param>
		/// <param name="initialDelay">the time to delay first execution.</param>
		/// <param name="delay">the delay between the termination of one execution and the commencement of the next.</param>
		/// <returns> a <see cref="IFuture{T}"/>  representing pending completion of the task,
		/// and whose <see cref="IFuture{T}.GetResult()"/> method will throw an exception upon
		/// cancellation.
		/// </returns>
		IScheduledFuture<Void> ScheduleWithFixedDelay(IRunnable command, TimeSpan initialDelay, TimeSpan delay);
	}
}