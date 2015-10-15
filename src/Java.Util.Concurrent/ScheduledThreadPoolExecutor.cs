using System;
using System.Collections.Generic;
using System.Timers;

namespace Java.Util.Concurrent
{
	/// <summary>
	/// A <see cref="ThreadPoolExecutor"/> that can additionally schedule
	/// commands to run after a given delay, or to execute
	/// periodically. This class is preferable to <see cref="Timer"/>
	/// when multiple worker threads are needed, or when the additional
	/// flexibility or capabilities of <see cref="ThreadPoolExecutor"/> (which
	/// this class extends) are required.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	public class ScheduledThreadPoolExecutor : ThreadPoolExecutor, IScheduledExecutorService
	{
		/// <summary> 
		/// Creates a new ScheduledThreadPoolExecutor with the given core
		/// pool size.
		/// </summary>
		/// <param name="corePoolSize">the number of threads to keep in the pool.</param>
		/// <exception cref="ArgumentException">if corePoolSize is less than 0</exception>
		public ScheduledThreadPoolExecutor( int corePoolSize ) : base(corePoolSize, Int32.MaxValue, new TimeSpan(0), Transform(new DelayQueue<IDelayed>()))
		{
			
		}
		/// <summary> 
		/// Creates a new ScheduledThreadPoolExecutor with the given core
		/// pool size and using the specified <see cref="IThreadFactory"/>
		/// </summary>
		/// <param name="corePoolSize"></param>
		/// <param name="threadFactory"></param>
		public ScheduledThreadPoolExecutor(int corePoolSize, IThreadFactory threadFactory): base(corePoolSize, Int32.MaxValue, new TimeSpan(0), Transform(new DelayQueue<IDelayed>()), threadFactory )
		{
			
		}

        private static IBlockingQueue<IRunnable> Transform(IBlockingQueue<IDelayed> delayQueue)
        {
            return new TransformingBlockingQueue<IDelayed, IRunnable>(delayQueue, d => (IRunnable) d, r => (IDelayed) r);
        }

		/// <summary> 
		/// Returns <c>true</c> if this executor has been shut down.
		/// </summary>
		/// <returns> 
		/// Returns <c>true</c> if this executor has been shut down.
		/// </returns>
		public override bool IsShutdown
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary> 
		/// Returns <c>true</c> if all tasks have completed following shut down.
		/// </summary>
		/// <remarks>
		/// Note that this will never return <c>true</c> unless
		/// either <see cref="Spring.Threading.Execution.IExecutorService.Shutdown()"/> or 
		/// <see cref="Spring.Threading.Execution.IExecutorService.ShutdownNow()"/> was called first.
		/// </remarks>
		/// <returns> <c>true</c> if all tasks have completed following shut down
		/// </returns>
		public override bool IsTerminated
		{
			get { throw new NotImplementedException(); }
		}

		/// <summary> 
		/// Initiates an orderly shutdown in which previously submitted
		/// tasks are executed, but no new tasks will be
		/// accepted. Invocation has no additional effect if already shut
		/// down.
		/// </summary>
		public override void Shutdown()
		{
			throw new NotImplementedException();
		}

		/// <summary> 
		/// Attempts to stop all actively executing tasks, halts the
		/// processing of waiting tasks, and returns a list of the tasks that were
		/// awaiting execution.
		/// </summary>
		/// <remarks> 
		/// There are no guarantees beyond best-effort attempts to stop
		/// processing actively executing tasks.  For example, typical
		/// implementations will cancel via <see cref="System.Threading.Thread.Interrupt()"/>, so if any
		/// tasks mask or fail to respond to interrupts, they may never terminate.
		/// </remarks>
		/// <returns> list of tasks that never commenced execution</returns>
		public override IList<IRunnable> ShutdownNow()
		{
			throw new NotImplementedException();
		}

		/// <summary> 
		/// Blocks until all tasks have completed execution after a shutdown
		/// request, or the timeout occurs, or the current thread is
		/// interrupted, whichever happens first. 
		/// </summary>
		/// <param name="timeSpan">the time span to wait.
		/// </param>
		/// <returns> <c>true</c> if this executor terminated and <c>false</c>
		/// if the timeout elapsed before termination
		/// </returns>
		public override bool AwaitTermination( TimeSpan timeSpan )
		{
			throw new NotImplementedException();
		}

		/// <summary> 
		/// Submits a value-returning task for execution and returns a
		/// Future representing the pending results of the task. The
		/// <see cref="IRunnable"/> method will return the task's result upon
		/// <b>successful</b> completion.
		/// </summary>
		/// <remarks> 
		/// If you would like to immediately block waiting
		/// for a task, you can use constructions of the form
		/// <code>
		///		result = exec.Submit(aCallable).GetResult();
		/// </code> 
		/// <p/> 
		/// Note: The <see cref="ICallable{T}"/> class includes a set of methods
		/// that can convert some other common closure-like objects,
		/// for example, <see cref="IFuture{T}"/> to
		/// <see cref="RejectedExecutionException"/> form so they can be submitted.
		/// </remarks>
		/// <param name="task">the task to submit</param>
		/// <returns> a <see cref="IFuture{T}.GetResult()"/> representing pending completion of the task</returns>
		/// <exception cref="Executors">if the task cannot be accepted for execution.</exception>
		/// <exception cref="System.ArgumentNullException">if the command is null</exception>
		public override IFuture<T> Submit<T>(ICallable<T> task )
		{
			throw new NotImplementedException();
		}

		/// <summary> 
		/// Submits a <see cref="RejectedExecutionException"/> task for execution and returns a
		/// <see cref="IFuture{T}"/> 
		/// representing that task. The <see cref="IRunnable"/> method will
		/// return the given result upon successful completion.
		/// </summary>
		/// <param name="task">the task to submit</param>
		/// <param name="result">the result to return</param>
		/// <returns> a <see cref="ArgumentNullException"/> representing pending completion of the task</returns>
		/// <exception cref="IFuture{T}.GetResult()">if the task cannot be accepted for execution.</exception>
		/// <exception cref="IFuture{T}">if the command is null</exception>
		public override IFuture<T> Submit<T>( IRunnable task, T result )
		{
			throw new NotImplementedException();
		}

		/// <summary> Submits a Runnable task for execution and returns a Future
		/// representing that task. The Future's <see cref="RejectedExecutionException"/> method will
		/// return <c>null</c> upon successful completion.
		/// </summary>
		/// <param name="task">the task to submit
		/// </param>
		/// <returns> a Future representing pending completion of the task
		/// </returns>
		/// <exception cref="ArgumentNullException">if the task cannot be accepted for execution.</exception>
		/// <exception cref="System.ArgumentNullException">if the command is null</exception>
		public override IFuture<Void> Submit( IRunnable task )
		{
			throw new NotImplementedException();
		}

		/// <summary> 
		/// Executes the given tasks, returning a list of <see cref="RejectedExecutionException"/>s holding
		/// their status and results when all complete.
		/// </summary>
		/// <remarks>
		/// <see cref="ArgumentNullException"/>
		/// is <c>true</c> for each element of the returned list.
		/// Note that a <b>completed</b> task could have
		/// terminated either normally or by throwing an exception.
		/// The results of this method are undefined if the given
		/// collection is modified while this operation is in progress.
		/// </remarks>
		/// <param name="tasks">the collection of tasks</param>
		/// <returns> A list of Futures representing the tasks, in the same
		/// sequential order as produced by the iterator for the given task
		/// list, each of which has completed.
		/// </returns>
		/// <exception cref="ICancellable.IsDone">if the task cannot be accepted for execution.</exception>
		/// <exception cref="IFuture{T}">if the command is null</exception>
		public override IList<IFuture<T>> InvokeAll<T>(IEnumerable<ICallable<T>> tasks )
		{
			throw new NotImplementedException();
		}

		/// <summary> 
		/// Executes the given tasks, returning a list of <see cref="ArgumentNullException"/>s holding
		/// their status and results when all complete or the <paramref name="durationToWait"/> expires, whichever happens first.
		/// </summary>
		/// <remarks>
		/// <see cref="RejectedExecutionException"/>
		/// is <c>true</c> for each element of the returned list.
		/// Note that a <b>completed</b> task could have
		/// terminated either normally or by throwing an exception.
		/// The results of this method are undefined if the given
		/// collection is modified while this operation is in progress.
		/// </remarks>
		/// <param name="tasks">the collection of tasks</param>
		/// <param name="durationToWait">the time span to wait.</param> 
		/// <returns> A list of Futures representing the tasks, in the same
		/// sequential order as produced by the iterator for the given
		/// task list. If the operation did not time out, each task will
		/// have completed. If it did time out, some of these tasks will
		/// not have completed.
		/// </returns>
		/// <exception cref="ICancellable.IsDone">if the task cannot be accepted for execution.</exception>
		/// <exception cref="IFuture{T}">if the command is null</exception>
		public override IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
		{
			throw new NotImplementedException();
		}

		/// <summary> 
		/// Executes the given tasks, returning the result
		/// of one that has completed successfully (i.e., without throwing
		/// an exception), if any do. 
		/// </summary>
		/// <remarks>
		/// Upon normal or exceptional return, tasks that have not completed are cancelled.
		/// The results of this method are undefined if the given
		/// collection is modified while this operation is in progress.
		/// </remarks>
		/// <param name="tasks">the collection of tasks</param>
		/// <returns> The result returned by one of the tasks.</returns>
		/// <exception cref="ArgumentNullException">if the task cannot be accepted for execution.</exception>
		/// <exception cref="RejectedExecutionException">if the command is null</exception>
		public override T InvokeAny<T>( IEnumerable<ICallable<T>> tasks )
		{
			throw new NotImplementedException();
		}

		/// <summary> 
		/// Creates and executes a one-shot action that becomes enabled
		/// after the given delay.
		/// </summary>
		/// <param name="command">the task to execute.</param>
		/// <param name="delay">the <see cref="IScheduledFuture{T}"/> from now to delay execution.</param>
		/// <returns> 
		/// a <see cref="TimeSpan"/> representing pending completion of the task,
		/// and whose <see cref="IFuture{T}.GetResult()"/> method will return <c>null</c>
		/// upon completion.
		/// </returns>
		public IScheduledFuture<Void> Schedule( IRunnable command, TimeSpan delay )
		{
			throw new NotImplementedException();
		}

		/// <summary> 
		/// Creates and executes a <see cref="IScheduledFuture{T}"/> that becomes enabled after the
		/// given delay.
		/// </summary>
		/// <param name="callable">the function to execute.</param>
		/// <param name="delay">the <see cref="IScheduledFuture{T}"/> from now to delay execution.</param>
		/// <returns> a <see cref="TimeSpan"/> that can be used to extract result or cancel.
		/// </returns>
		public IScheduledFuture<T> Schedule<T>(ICallable<T> callable, TimeSpan delay )
		{
			throw new NotImplementedException();
		}

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
		public IScheduledFuture<Void> ScheduleAtFixedRate( IRunnable command, TimeSpan initialDelay, TimeSpan period )
		{
			throw new NotImplementedException();
		}

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
		/// <returns> a <see cref="IFuture{T}.GetResult()"/>  representing pending completion of the task,
		/// and whose <see cref="IFuture{T}"/> method will throw an exception upon
		/// cancellation.
		/// </returns>
		public IScheduledFuture<Void> ScheduleWithFixedDelay( IRunnable command, TimeSpan initialDelay, TimeSpan delay )
		{
			throw new NotImplementedException();
		}

		/// <summary> Executes the given tasks, returning the result
		/// of one that has completed successfully (i.e., without throwing
		/// an exception), if any do before the given timeout elapses.
		/// </summary>
		/// <remarks>
		/// Upon normal or exceptional return, tasks that have not
		/// completed are cancelled.
		/// The results of this method are undefined if the given
		/// collection is modified while this operation is in progress.
		/// </remarks>
		/// <param name="tasks">the collection of tasks</param>
		/// <param name="durationToWait">the time span to wait.</param> 
		/// <returns> The result returned by one of the tasks.
		/// </returns>
		/// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
		/// <exception cref="System.ArgumentNullException">if the command is null</exception>
		public override T InvokeAny<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
		{
			throw new NotImplementedException();
		}

		/// <summary> 
		/// Executes the given command at some time in the future.
		/// </summary>
		/// <remarks>
		/// The command may execute in a new thread, in a pooled thread, or in the calling
		/// thread, at the discretion of the <see cref="ArgumentNullException"/> implementation.
		/// </remarks>
		/// <param name="command">the runnable task</param>
		/// <exception cref="IExecutor">if the task cannot be accepted for execution.</exception>
		/// <exception cref="RejectedExecutionException">if the command is null</exception>
		public override void Execute( IRunnable command )
		{
			throw new NotImplementedException();
		}
	}
}