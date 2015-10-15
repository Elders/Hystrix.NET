#region License
/*
* Copyright (C) 2002-2009 the original author or authors.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*      http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion

using System;
using System.Threading;

namespace Java.Util.Concurrent
{
	/// <summary>
	/// Enumeration representing a task execution status.
	/// </summary>
	[Flags]
	internal enum TaskState : short // NET_ONLY
	{
		/// <summary>State value representing that task is ready to run </summary>
        Ready = 0,
		/// <summary>State value representing that task is running </summary>
		Running = 1,
		/// <summary>State value representing that task ran </summary>
		Complete = 2,
		/// <summary>State value representing that task was cancelled </summary>
		Cancelled = 4,
		/// <summary>State value representing that the task should be stopped.</summary>
		Stop = 8
	}

	/// <summary> 
	/// A cancellable asynchronous computation.  
	/// </summary>	
	/// <remarks> 
	/// <para>
	/// This class provides a base implementation of 
	/// <see cref="IFuture{T}"/> , with methods to start and cancel
	/// a computation, query to see if the computation is complete, and
	/// retrieve the result of the computation.  The result can only be
	/// retrieved when the computation has completed; the <see cref="GetResult()"/>
	/// method will block if the computation has not yet completed.  Once
	/// the computation has completed, the computation cannot be restarted
	/// or cancelled.
	/// </para>
	/// <para>
	/// A <see cref="FutureTask{T}"/> can be used to wrap a <see cref="Action"/>
	/// delegate, <see cref="Func{T}"/> delegate, <see cref="IRunnable"/> object 
	/// or <see cref="ICallable{T}"/> object.  Because <see cref="FutureTask{T}"/>
	/// implements <see cref="IRunnable"/>, a <see cref="FutureTask{T}"/> can be
	/// submitted to an <see cref="IExecutor"/> for execution.
	/// </para>
	/// <para>
	/// In addition to serving as a standalone class, this class provides
	/// protected functionality that may be useful when creating
	/// customized task classes.
	/// </para>
	/// </remarks>
    /// <typeparam name="T">
    /// The result type returned by <see cref="GetResult()"/> method.
    /// </typeparam>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio (.NET)</author>
    /// <author>Kenneth Xu</author>
    public class FutureTask<T> : IRunnableFuture<T>, IContextCopyingTask //BACKPORT_3_1
	{
	    private readonly ICallable<T> _callable;
	    private T _result;
	    private Exception _exception;
	    private TaskState _taskState;

	    /// <summary> 
	    /// The thread running task. When nulled after set/cancel, this
	    /// indicates that the results are accessible.  Must be
	    /// volatile, to ensure visibility upon completion.
	    /// </summary>
	    private volatile Thread _runningThread;

	    private IContextCarrier _contextCarrier;

	    /// <summary> 
        /// Creates a <see cref="FutureTask{T}"/> that will, upon running, execute the
        /// given <see cref="ICallable{T}"/>.
        /// </summary>
        /// <param name="callable">The callable task.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="callable"/> is <c>null</c>.
        /// </exception>
        public FutureTask(ICallable<T> callable)
        {
            if(callable==null) throw new ArgumentNullException("callable");
	        _callable = callable;
        }

        /// <summary> 
        /// Creates a <see cref="FutureTask{T}"/> that will, upon running, execute the
        /// given <see cref="Func{T}"/> delegate.
        /// </summary>
        /// <param name="call">The <see cref="Func{T}"/> delegate.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="call"/> is <c>null</c>.
        /// </exception>
        public FutureTask(Func<T> call)
            : this(Executors.CreateCallable(call))
        {
        }

        /// <summary> 
        /// Creates a <see cref="FutureTask{T}"/> that will, upon running, execute the
        /// given <see cref="IRunnable"/>, and arrange that <see cref="GetResult()"/> 
        /// will return the given <paramref name="result"/> upon successful completion.
        /// </summary>
        /// <param name="task">The runnable task.</param>
        /// <param name="result">
        /// The result to return on successful completion. If
        /// you don't need a particular result, consider using
        /// constructions of the form:
        /// <code language="c#">
        ///		Future f = new FutureTask(runnable, default(T))
        ///	</code>	
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="task"/> is <c>null</c>.
        /// </exception>
        public FutureTask(IRunnable task, T result)
            : this(Executors.CreateCallable(task, result))
        {
        }

        /// <summary> 
        /// Creates a <see cref="FutureTask{T}"/> that will, upon running, execute the
        /// given <see cref="Action"/>, and arrange that <see cref="GetResult()"/> 
        /// will return the given <paramref name="result"/> upon successful completion.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> delegate.</param>
        /// <param name="result">
        /// The result to return on successful completion. If
        /// you don't need a particular result, consider using
        /// constructions of the form:
        /// <code language="c#">
        ///		Future f = new FutureTask(action, default(T))
        ///	</code>	
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="action"/> is <c>null</c>.
        /// </exception>
        public FutureTask(Action action, T result)
            : this(Executors.CreateCallable(action, result))
        {
        }

	    #region IFuture<T> Members

        /// <summary>
	    /// Determines if this task was cancelled.
	    /// </summary>
	    /// <remarks> 
	    /// Returns <c>true</c> if this task was cancelled before it completed
	    /// normally.
	    /// </remarks>
	    /// <returns> <c>true</c>if task was cancelled before it completed
	    /// </returns>
	    public virtual bool IsCancelled
	    {
	        get
	        {
	            lock (this)
	            {
	                return _taskState == TaskState.Cancelled;
	            }
	        }
	    }

	    /// <summary> 
	    /// Returns <c>true</c> if this task completed.
	    /// </summary>
	    /// <remarks> 
	    /// Completion may be due to normal termination, an exception, or
	    /// cancellation -- in all of these cases, this method will return
	    /// <c>true</c> if this task completed.
	    /// </remarks>
	    /// <returns> <c>true</c>if this task completed.</returns>
	    public virtual bool IsDone
	    {
	        get
	        {
	            lock (this)
	            {
	                return RanOrCancelled() && _runningThread == null;
	            }
	        }
	    }

	    /// <summary>
	    /// Waits for computation to complete, then returns its result. 
	    /// </summary>
	    /// <remarks> 
	    /// Waits if necessary for the computation to complete, and then
	    /// retrieves its result.
	    /// </remarks>
	    /// <returns>The computed result</returns>
	    /// <exception cref="Spring.Threading.Execution.CancellationException">if the computation was cancelled.</exception>
	    /// <exception cref="Spring.Threading.Execution.ExecutionException">if the computation threw an exception.</exception>
	    /// <exception cref="System.Threading.ThreadInterruptedException">if the current thread was interrupted while waiting.</exception>
	    public virtual T Get()
        {
            lock (this)
            {
                WaitFor();
                return Result;
            }
        }

	    /// <summary>
	    /// Waits for the given time span, then returns its result.
	    /// </summary>
	    /// <remarks> 
	    /// Waits, if necessary, for at most the <paramref name="durationToWait"/> for the computation
	    /// to complete, and then retrieves its result, if available.
	    /// </remarks>
	    /// <param name="durationToWait">the <see cref="System.TimeSpan"/> to wait.</param>
	    /// <returns>the computed result</returns>
	    /// <exception cref="Spring.Threading.Execution.CancellationException">if the computation was cancelled.</exception>
	    /// <exception cref="Spring.Threading.Execution.ExecutionException">if the computation threw an exception.</exception>
	    /// <exception cref="System.Threading.ThreadInterruptedException">if the current thread was interrupted while waiting.</exception>
	    /// <exception cref="TimeoutException">if the computation threw an exception.</exception>
	    public virtual T Get(TimeSpan durationToWait)
        {
            lock (this)
            {
                WaitFor(durationToWait);
                return Result;
            }
        }

	    /// <summary> 
	    /// Attempts to cancel execution of this task.  
	    /// </summary>
	    /// <remarks> 
	    /// This attempt will fail if the task has already completed, already been cancelled,
	    /// or could not be cancelled for some other reason. If successful,
        /// and this task has not started when <see cref="ICancellable.Cancel()"/> is called,
	    /// this task should never run.  If the task has already started, the in-progress tasks are allowed
	    /// to complete
	    /// </remarks>
	    /// <returns> <c>false</c> if the task could not be cancelled,
	    /// typically because it has already completed normally;
	    /// <c>true</c> otherwise
	    /// </returns>
	    public virtual bool Cancel()
	    {
	        return Cancel(false);
	    }

	    /// <summary> 
	    /// Attempts to cancel execution of this task.  
	    /// </summary>
	    /// <remarks> 
	    /// This attempt will fail if the task has already completed, already been cancelled,
	    /// or could not be cancelled for some other reason. If successful,
        /// and this task has not started when <see cref="ICancellable.Cancel()"/> is called,
	    /// this task should never run.  If the task has already started,
	    /// then the <paramref name="mayInterruptIfRunning"/> parameter determines
	    /// whether the thread executing this task should be interrupted in
	    /// an attempt to stop the task.
	    /// </remarks>
	    /// <param name="mayInterruptIfRunning"><c>true</c> if the thread executing this
	    /// task should be interrupted; otherwise, in-progress tasks are allowed
	    /// to complete
	    /// </param>
	    /// <returns> <c>false</c> if the task could not be cancelled,
	    /// typically because it has already completed normally;
	    /// <c>true</c> otherwise
	    /// </returns>
	    public virtual bool Cancel(bool mayInterruptIfRunning)
	    {
	        lock (this)
	        {
	            if (RanOrCancelled()) return false;
	            _taskState = TaskState.Cancelled;
	            if (mayInterruptIfRunning)
	            {
	                Thread r = _runningThread;
	                if (r != null) r.Interrupt();
	            }
	            _runningThread = null;
	            Monitor.PulseAll(this);
	        }
	        Done();
	        return true;
	    }

	    /// <summary>
	    /// The entry point
	    /// </summary>
	    public virtual void Run()
	    {
            if (_contextCarrier != null)
            {
                _contextCarrier.Restore();
            }
	        lock (this)
	        {
	            if (_taskState != TaskState.Ready) return;
	            _taskState = TaskState.Running;
	            _runningThread = Thread.CurrentThread;
	        }
	        try
	        {
	            SetCompleted(_callable.Call());
	        }
	        catch (Exception ex)
	        {
	            SetFailed(ex);
	        }
	    }

        #endregion

        #region Protected Methods

        /// <summary> 
        /// Sets the result of this <see cref="IFuture{T}"/> to the given 
        /// <paramref name="result"/> value unless
        /// this future has already been set or has been cancelled.
        /// </summary>
        /// <remarks>
        /// This method is invoked upon successful completion of the 
        /// computation.
        /// </remarks>
        /// <param name="result">
        /// The value to be retured by <see cref="GetResult()"/>.
        /// </param>
        protected virtual void SetResult(T result)
		{
            SetCompleted(result);
        }

	    /// <summary> 
	    /// Protected method invoked when this task transitions to state
	    /// <see cref="ICancellable.IsDone"/> (whether normally or via cancellation). 
	    /// </summary>
	    /// <remarks> 
	    /// The default implementation does nothing.  Subclasses may override
	    /// this method to invoke completion callbacks or perform
	    /// bookkeeping. Note that you can query status inside the
	    /// implementation of this method to determine whether this task
	    /// has been cancelled.
	    /// </remarks>
	    protected internal virtual void Done()
	    {
	    }

	    /// <summary> 
	    /// Causes this future to report an <see cref="Spring.Threading.Execution.ExecutionException"/> 
	    /// with the given <see cref="System.Exception"/> as its cause, unless this <see cref="IFuture{T}"/> has
	    /// already been set or has been cancelled.
	    /// </summary>
	    /// <remarks>
	    /// This method is invoked internally by the <see cref="Spring.Threading.IRunnable"/> method
	    /// upon failure of the computation.
	    /// </remarks>
	    /// <param name="t">the cause of failure</param>
	    protected virtual void SetException(Exception t)
	    {
	        SetFailed(t);
	    }

	    /// <summary> 
	    /// Executes the computation without setting its result, and then
	    /// resets this Future to initial state, failing to do so if the
	    /// computation encounters an exception or is cancelled.  
	    /// </summary>
	    /// <remarks>
	    /// This is designed for use with tasks that intrinsically execute more
	    /// than once.
	    /// </remarks>
	    /// <returns> <c>true</c> if successfully run and reset</returns>
	    protected virtual bool RunAndReset()
	    {
	        lock (this)
	        {
	            if (_taskState != TaskState.Ready) return false;
	            _taskState = TaskState.Running;
	            _runningThread = Thread.CurrentThread;
	        }
	        try
	        {
	            _callable.Call();
	            lock (this)
	            {
	                _runningThread = null;
	                if (_taskState == TaskState.Running)
	                {
	                    _taskState = TaskState.Ready;
	                    return true;
	                }
	                else
	                {
	                    return false;
	                }
	            }
	        }
	        catch (Exception ex)
	        {
	            SetFailed(ex);
	            return false;
	        }
	    }

        #endregion

	    /// <summary>
	    /// Sets the result of the task, and marks the task as completed
	    /// </summary>
	    private void SetCompleted(T value)
	    {
	        lock (this)
	        {
	            if (RanOrCancelled()) return;
	            _taskState = TaskState.Complete;
	            _result = value;
	            _runningThread = null;
	            Monitor.PulseAll(this);
	        }

	        // invoking callbacks *after* setting future as completed and
	        // outside the synchronization block makes it safe to call
	        // interrupt() from within callback code (in which case it will be
	        // ignored rather than cause deadlock / illegal state exception)
	        Done();
	    }

	    /// <summary>
	    /// Sets the exception result of the task, and marks the tasks as completed.
	    /// </summary>
	    private void SetFailed(Exception value)
	    {
	        lock (this)
	        {
	            if (RanOrCancelled()) return;
	            _taskState = TaskState.Complete;
	            _exception = value;
	            _runningThread = null;
	            Monitor.PulseAll(this);
	        }

	        // invoking callbacks *after* setting future as completed and
	        // outside the synchronization block makes it safe to call
	        // interrupt() from within callback code (in which case it will be
	        // ignored rather than cause deadlock / illegal state exception)
	        Done();
	    }

	    /// <summary> 
	    /// Gets the result of the task.
	    /// </summary>
	    private T Result
	    {
	        get
	        {
	            if (_taskState == TaskState.Cancelled)
	            {
	                throw new CancellationException();
	            }
	            if (_exception != null)
	            {
	                throw new ExecutionException(_exception);
	            }
	            return _result;
	        }

	    }

	    /// <summary> Waits for the task to complete.</summary>
	    private void WaitFor()
	    {
	        while (!IsDone)
	        {
	            Monitor.Wait(this);
	        }
	    }

	    /// <summary> 
	    /// Waits for the task to complete for <paramref name="durationToWait"/> or throws a
	    /// <see cref="TimeoutException"/>
	    /// if still not completed after that
	    /// </summary>
	    private void WaitFor(TimeSpan durationToWait)
	    {
	        if (IsDone) return;

            if (durationToWait.Ticks < 0)
            {
                while (!IsDone)
                {
                    Monitor.Wait(this);
                }
            }
            else
            {
                DateTime deadline = DateTime.UtcNow.Add(durationToWait);
                while (durationToWait.Ticks > 0)
                {
                    Monitor.Wait(this, durationToWait);
                    if (IsDone) return;
                    durationToWait = deadline.Subtract(DateTime.UtcNow);
                }
                throw new TimeoutException();
            }
	    }

	    private const TaskState CompleteOrCancelled = TaskState.Complete | TaskState.Cancelled;
	    private bool RanOrCancelled()
	    {
            return (_taskState & CompleteOrCancelled) != 0;
	    }

        #region IContextCopyingTask Members

        IContextCarrier IContextCopyingTask.ContextCarrier
        {
            get { return _contextCarrier; }
            set { _contextCarrier = value; }
        }

        #endregion
    }
}