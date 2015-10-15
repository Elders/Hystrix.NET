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
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Java.Util.Concurrent
{
    /// <summary> 
    /// Provides default implementations of <see cref="Spring.Threading.Execution.IExecutorService"/>
    /// execution methods. 
    /// </summary>
    /// <remarks> 
    /// This class implements the <see cref="Spring.Threading.Execution.IExecutorService"/> methods using a
    /// <see cref="IRunnableFuture{T}"/> returned by NewTaskFor
    /// , which defaults to the <see cref="FutureTask{T}"/> class provided in this package.  
    /// <p/>
    /// For example, the implementation of <see cref="Spring.Threading.Execution.IExecutorService.Submit(IRunnable)"/> creates an
    /// associated <see cref="IRunnableFuture{T}"/> that is executed and
    /// returned. Subclasses may override the NewTaskFor methods
    /// to return <see cref="IRunnableFuture{T}"/> implementations other than
    /// <see cref="FutureTask{T}"/>.
    /// 
    /// <p/> 
    /// <b>Extension example</b>. 
    /// Here is a sketch of a class
    /// that customizes <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> to use
    /// a custom Task class instead of the default <see cref="FutureTask{T}"/>:
    /// <code>
    /// public class CustomThreadPoolExecutor : ThreadPoolExecutor {
    ///		class CustomTask : IRunnableFuture {...}
    /// 
    ///		protected IRunnableFuture newTaskFor(ICallable c) {
    ///			return new CustomTask(c);
    /// 	}
    ///		protected IRunnableFuture newTaskFor(IRunnable r) {
    /// 		return new CustomTask(r);
    /// 	}
    /// 	// ... add constructors, etc.
    /// }
    /// </code>
    /// </remarks>
    /// <author>Doug Lea</author>
    /// <author>Griffin Caprio(.NET)</author>
    /// <author>Kenneth Xu</author>
    public abstract class AbstractExecutorService : IExecutorService //JDK_1_6
    {
        private IContextCarrierFactory _contextCarrierFactory;

        /// <summary>
        /// Gets and sets the context carrier factory
        /// </summary>
        public IContextCarrierFactory ContextCarrierFactory
        {
            get { return _contextCarrierFactory; }
            set { _contextCarrierFactory = value; }
        }

        /// <summary>
        /// Occurs when an untrapped thread exception is thrown.
        /// </summary>
        public event ThreadExceptionEventHandler ThreadException;


        /// <summary>
        /// Calls <see cref="Shutdown"/> to complete all pending tasks and shutdown service.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            Shutdown();
        }

        #region Abstract Methods

        /// <summary> 
        /// Initiates an orderly shutdown in which previously submitted
        /// tasks are executed, but no new tasks will be
        /// accepted. Invocation has no additional effect if already shut
        /// down.
        /// </summary>
        public abstract void Shutdown();

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
        public abstract IList<IRunnable> ShutdownNow();

        /// <summary> 
        /// Executes the given task sometime in the future.  The task may 
        /// execute in a new thread or in an existing pooled thread.
        /// </summary>
        /// <remarks>
        /// If the task cannot be submitted for execution, either because this
        /// executor has been shutdown or because its capacity has been reached,
        /// the task is handled by the current <see cref="IRejectedExecutionHandler"/>
        /// for this <see cref="ThreadPoolExecutor"/>.
        /// </remarks>
        /// <param name="command">the task to execute</param>
        /// <exception cref="RejectedExecutionException">
        /// if the task cannot be accepted. 
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// if <paramref name="command"/> is <c>null</c>
        /// </exception>
        protected abstract void DoExecute(IRunnable command);

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
        public abstract bool AwaitTermination(TimeSpan timeSpan);

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
        public abstract bool IsTerminated { get; }

        /// <summary> 
        /// Returns <c>true</c> if this executor has been shut down.
        /// </summary>
        /// <returns> 
        /// Returns <c>true</c> if this executor has been shut down.
        /// </returns>
        public abstract bool IsShutdown { get; }

        #endregion

        #region Execute Methods

        /// <summary> 
        /// Executes the given task at some time in the future.
        /// </summary>
        /// <remarks>
        /// The task may execute in a new thread, in a pooled thread, or in the calling
        /// thread, at the discretion of the <see cref="IExecutor"/> implementation.
        /// </remarks>
        /// <param name="action">The task to be executed.</param>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">
        /// If the task cannot be accepted for execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="action"/> is <c>null</c>
        /// </exception>
        public virtual void Execute(Action action)
        {
            Execute(Executors.CreateRunnable(action));
        }

        /// <summary> 
        /// Executes the given command at some time in the future.
        /// </summary>
        /// <remarks>
        /// The command may execute in a new thread, in a pooled thread, or in the calling
        /// thread, at the discretion of the <see cref="Spring.Threading.IExecutor"/> implementation.
        /// </remarks>
        /// <param name="runnable">the runnable task</param>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
        /// <exception cref="System.ArgumentNullException">if the command is null</exception>	
        public virtual void Execute(IRunnable runnable)
        {
            if (runnable == null)
            {
                throw new ArgumentNullException("runnable");
            }

            DoExecute(CaptureContext(runnable));
        }

        #endregion Execute Methods

        #region Submit Methods

        /// <summary> 
        /// Submits a Runnable task for execution and returns a Future
        /// representing that task. The Future's <see cref="IFuture{T}.Get()"/> method will
        /// return <c>null</c> upon successful completion.
        /// </summary>
        /// <param name="runnable">the task to submit
        /// </param>
        /// <returns> a Future representing pending completion of the task
        /// </returns>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
        public virtual IFuture<Void> Submit(IRunnable runnable)
        {
            return Submit<Void>(runnable, null);
        }

        /// <summary> 
        /// Submits a delegate <see cref="Action"/> for execution and returns an
        /// <see cref="IFuture{T}"/> representing that <paramref name="action"/>. The 
        /// <see cref="IFuture{T}.Get()"/> method will return <c>null</c>.
        /// </summary>
        /// <param name="action">The task to submit.</param>
        /// <returns>
        /// An <see cref="IFuture{T}"/> representing pending completion of the 
        /// <paramref name="action"/>.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the <paramref name="action"/> cannot be accepted for execution.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="action"/> is <c>null</c>
        /// </exception>
        public virtual IFuture<Void> Submit(Action action)
        {
            return Submit<Void>(action, null);
        }

        /// <summary> 
        /// Submits a delegate <see cref="Func{T}"/> for execution and returns a
        /// <see cref="IFuture{T}"/> representing that <paramref name="call"/>. 
        /// The <see cref="IFuture{T}.Get()"/> method will return the 
        /// result of <paramref name="call"/><c>()</c> upon successful completion.
        /// </summary>
        /// <param name="call">The task to submit.</param>
        /// <returns>
        /// An <see cref="IFuture{T}"/> representing pending completion of the
        /// <paramref name="call"/>.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the <paramref name="call"/> cannot be accepted for execution.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="call"/> is <c>null</c>.
        /// </exception>
        public virtual IFuture<T> Submit<T>(Func<T> call)
        {
            if (call == null) throw new ArgumentNullException("call");
            return Submit(NewTaskFor(call));
        }
        
        /// <summary> 
        /// Submits a delegate <see cref="Action"/> for execution and returns a
        /// <see cref="IFuture{T}"/> representing that <paramref name="action"/>. 
        /// The <see cref="IFuture{T}.Get()"/> method will return the 
        /// given <paramref name="result"/> upon successful completion.
        /// </summary>
        /// <param name="action">The task to submit.</param>
        /// <param name="result">The result to return.</param>
        /// <returns>
        /// An <see cref="IFuture{T}"/> representing pending completion of the
        /// <paramref name="action"/>.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the <paramref name="action"/> cannot be accepted for execution.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="action"/> is <c>null</c>.
        /// </exception>
        public virtual IFuture<T> Submit<T>(Action action, T result)
        {
            if (action == null) throw new ArgumentNullException("action");
            return Submit(NewTaskFor(action, result));
        }

        /// <summary> 
        /// Submits a <see cref="ICallable{T}"/> for execution and returns a
        /// <see cref="IFuture{T}"/> representing that <paramref name="callable"/>. 
        /// The <see cref="IFuture{T}.Get()"/> method will return the 
        /// result of <see cref="ICallable{T}.Call"/> upon successful completion.
        /// </summary>
        /// <param name="callable">The task to submit.</param>
        /// <returns>
        /// An <see cref="IFuture{T}"/> representing pending completion of the
        /// <paramref name="callable"/>.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the <paramref name="callable"/> cannot be accepted for execution.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="callable"/> is <c>null</c>.
        /// </exception>
        public virtual IFuture<T> Submit<T>(ICallable<T> callable)
        {
            if (callable == null) throw new ArgumentNullException("callable");
            return Submit(NewTaskFor(callable));
        }

        /// <summary> 
        /// Submits a <see cref="IRunnable"/> task for execution and returns a
        /// <see cref="IFuture{T}"/> representing that <paramref name="runnable"/>. 
        /// The <see cref="IFuture{T}.Get()"/> method will return the 
        /// given <paramref name="result"/> upon successful completion.
        /// </summary>
        /// <param name="runnable">The task to submit.</param>
        /// <param name="result">The result to return.</param>
        /// <returns>
        /// An <see cref="IFuture{T}"/> representing pending completion of the
        /// <paramref name="runnable"/>.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the <paramref name="runnable"/> cannot be accepted for execution.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="runnable"/> is <c>null</c>.
        /// </exception>
        public virtual IFuture<T> Submit<T>(IRunnable runnable, T result)
        {
            if (runnable == null) throw new ArgumentNullException("runnable");
            return Submit(NewTaskFor(runnable, result));
        }

        #endregion Submit Methods

        #region InvokeAny<T> Methods

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning the result
        /// of one that has completed successfully (i.e., without throwing
        /// an exception), if any do. 
        /// </summary>
        /// <remarks>
        /// Upon normal or exceptional return, <paramref name="tasks"/> that 
        /// have not completed are cancelled.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="ICollection{T}">collection</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <returns>The result returned by one of the tasks.</returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual T InvokeAny<T>(IEnumerable<ICallable<T>> tasks)
        {
            ICollection<ICallable<T>> collection = tasks as ICollection<ICallable<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAny(tasks, count, false, TimeSpan.Zero, Callable2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning the result
        /// of one that has completed successfully (i.e., without throwing
        /// an exception), if any do. 
        /// </summary>
        /// <remarks>
        /// Upon normal or exceptional return, <paramref name="tasks"/> that 
        /// have not completed are cancelled.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="ICollection{T}">collection</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <returns>The result returned by one of the tasks.</returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual T InvokeAny<T>(params ICallable<T>[] tasks)
        {
            return InvokeAny((IEnumerable<ICallable<T>>) tasks);
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning the result
        /// of one that has completed successfully (i.e., without throwing
        /// an exception), if any do. 
        /// </summary>
        /// <remarks>
        /// Upon normal or exceptional return, <paramref name="tasks"/> that 
        /// have not completed are cancelled.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="ICollection{T}">collection</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <returns>The result returned by one of the tasks.</returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual T InvokeAny<T>(IEnumerable<Func<T>> tasks)
        {
            ICollection<Func<T>> collection = tasks as ICollection<Func<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAny(tasks, count, false, TimeSpan.Zero, Call2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning the result
        /// of one that has completed successfully (i.e., without throwing
        /// an exception), if any do. 
        /// </summary>
        /// <remarks>
        /// Upon normal or exceptional return, <paramref name="tasks"/> that 
        /// have not completed are cancelled.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="ICollection{T}">collection</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <returns>The result returned by one of the tasks.</returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual T InvokeAny<T>(params Func<T>[] tasks)
        {
            return InvokeAny((IEnumerable<Func<T>>) tasks);
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning the result
        /// of one that has completed successfully (i.e., without throwing
        /// an exception), if any do before the given 
        /// <paramref name="durationToWait"/> elapses.
        /// </summary>
        /// <remarks>
        /// Upon normal or exceptional return, <paramref name="tasks"/> that 
        /// have not completed are cancelled.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="ICollection{T}">collection</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>The result returned by one of the tasks.</returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual T InvokeAny<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
        {
            ICollection<ICallable<T>> collection = tasks as ICollection<ICallable<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAny(tasks, count, true, durationToWait, Callable2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning the result
        /// of one that has completed successfully (i.e., without throwing
        /// an exception), if any do before the given 
        /// <paramref name="durationToWait"/> elapses.
        /// </summary>
        /// <remarks>
        /// Upon normal or exceptional return, <paramref name="tasks"/> that 
        /// have not completed are cancelled.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="ICollection{T}">collection</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>The result returned by one of the tasks.</returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual T InvokeAny<T>(TimeSpan durationToWait, params ICallable<T>[] tasks)
        {
            return InvokeAny(durationToWait, (IEnumerable<ICallable<T>>) tasks);
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning the result
        /// of one that has completed successfully (i.e., without throwing
        /// an exception), if any do before the given 
        /// <paramref name="durationToWait"/> elapses.
        /// </summary>
        /// <remarks>
        /// Upon normal or exceptional return, <paramref name="tasks"/> that 
        /// have not completed are cancelled.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="ICollection{T}">collection</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>The result returned by one of the tasks.</returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual T InvokeAny<T>(TimeSpan durationToWait, IEnumerable<Func<T>> tasks)
        {
            ICollection<Func<T>> collection = tasks as ICollection<Func<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAny(tasks, count, true, durationToWait, Call2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning the result
        /// of one that has completed successfully (i.e., without throwing
        /// an exception), if any do before the given 
        /// <paramref name="durationToWait"/> elapses.
        /// </summary>
        /// <remarks>
        /// Upon normal or exceptional return, <paramref name="tasks"/> that 
        /// have not completed are cancelled.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="ICollection{T}">collection</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>The result returned by one of the tasks.</returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual T InvokeAny<T>(TimeSpan durationToWait, params Func<T>[] tasks)
        {
            return InvokeAny(durationToWait, (IEnumerable<Func<T>>) tasks);
        }

        #endregion InvokeAny<T> Methods

        #region InvokeAll<T> Methods

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their status and results when all complete.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="ICancellable.IsDone"/> is <c>true</c> for each element of 
        /// the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// A <b>completed</b> task could have
        /// terminated either normally or by throwing an exception.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list, each of which has completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
	    public virtual IList<IFuture<T>> InvokeAll<T>(IEnumerable<ICallable<T>> tasks)
        {
            ICollection<ICallable<T>> collection = tasks as ICollection<ICallable<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAll(tasks, count, Callable2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their status and results when all complete.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="ICancellable.IsDone"/> is <c>true</c> for each element of 
        /// the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// A <b>completed</b> task could have
        /// terminated either normally or by throwing an exception.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list, each of which has completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAll<T>(params ICallable<T>[] tasks)
        {
            return InvokeAll((IEnumerable<ICallable<T>>) tasks);
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their status and results when all complete.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="ICancellable.IsDone"/> is <c>true</c> for each element of 
        /// the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// A <b>completed</b> task could have
        /// terminated either normally or by throwing an exception.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list, each of which has completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
	    public virtual IList<IFuture<T>> InvokeAll<T>(IEnumerable<Func<T>> tasks)
        {
            ICollection<Func<T>> collection = tasks as ICollection<Func<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAll(tasks, count, Call2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their status and results when all complete.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="ICancellable.IsDone"/> is <c>true</c> for each element of 
        /// the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// A <b>completed</b> task could have
        /// terminated either normally or by throwing an exception.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list, each of which has completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAll<T>(params Func<T>[] tasks)
        {
            return InvokeAll((IEnumerable<Func<T>>) tasks);
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their status and results when all complete or the
        /// <paramref name="durationToWait"/> expires, whichever happens
        /// first.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="ICancellable.IsDone"/> is <c>true</c> for each element of 
        /// the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// A <b>completed</b> task could have
        /// terminated either normally or by throwing an exception.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list. If the operation did not time out, each task will
        /// have completed. If it did time out, some of these tasks will
        /// not have completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
        {
            ICollection<ICallable<T>> collection = tasks as ICollection<ICallable<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAll(tasks, count, durationToWait, Callable2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their status and results when all complete or the
        /// <paramref name="durationToWait"/> expires, whichever happens
        /// first.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="ICancellable.IsDone"/> is <c>true</c> for each element of 
        /// the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// A <b>completed</b> task could have
        /// terminated either normally or by throwing an exception.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list. If the operation did not time out, each task will
        /// have completed. If it did time out, some of these tasks will
        /// not have completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, params ICallable<T>[] tasks)
        {
            return InvokeAll(durationToWait, (IEnumerable<ICallable<T>>) tasks);
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their status and results when all complete or the
        /// <paramref name="durationToWait"/> expires, whichever happens
        /// first.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="ICancellable.IsDone"/> is <c>true</c> for each element of 
        /// the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// A <b>completed</b> task could have
        /// terminated either normally or by throwing an exception.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list. If the operation did not time out, each task will
        /// have completed. If it did time out, some of these tasks will
        /// not have completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
	    public virtual IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, IEnumerable<Func<T>> tasks)
        {
            ICollection<Func<T>> collection = tasks as ICollection<Func<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAll(tasks, count, durationToWait, Call2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their status and results when all complete or the
        /// <paramref name="durationToWait"/> expires, whichever happens
        /// first.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="ICancellable.IsDone"/> is <c>true</c> for each element of 
        /// the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// A <b>completed</b> task could have
        /// terminated either normally or by throwing an exception.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list. If the operation did not time out, each task will
        /// have completed. If it did time out, some of these tasks will
        /// not have completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAll<T>(TimeSpan durationToWait, params Func<T>[] tasks)
	    {
	        return InvokeAll(durationToWait, (IEnumerable<Func<T>>) tasks);
        }

        #endregion InvokeAll<T> Methods

        #region InvokeAllOrFail<T> Methods

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their results when all complete successfully or throws
        /// exception when any one fails.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Upon successful return, <see cref="ICancellable.IsDone"/> is 
        /// <c>true</c> and <see cref="IFuture{T}.Get()"/> is guaranteed
        /// to success for each element of the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// The method returns immediately when any one of the tasks throws 
        /// exception. When this happens, all uncompleted tasks are cancelled
        /// and the exception is thrown.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the enumerator for the given 
        /// task list, each of which has completed successfully.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAllOrFail<T>(IEnumerable<ICallable<T>> tasks)
        {
            ICollection<ICallable<T>> collection = tasks as ICollection<ICallable<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAllOrFail(tasks, count, false, TimeSpan.Zero, Callable2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their results when all complete successfully or throws
        /// exception when any one fails.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Upon successful return, <see cref="ICancellable.IsDone"/> is 
        /// <c>true</c> and <see cref="IFuture{T}.Get()"/> is guaranteed
        /// to success for each element of the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// The method returns immediately when any one of the tasks throws 
        /// exception. When this happens, all uncompleted tasks are cancelled
        /// and the exception is thrown.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list, each of which has completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAllOrFail<T>(params ICallable<T>[] tasks)
        {
            return InvokeAllOrFail((IEnumerable<ICallable<T>>)tasks);
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their results when all complete successfully or throws
        /// exception when any one fails.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Upon successful return, <see cref="ICancellable.IsDone"/> is 
        /// <c>true</c> and <see cref="IFuture{T}.Get()"/> is guaranteed
        /// to success for each element of the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// The method returns immediately when any one of the tasks throws 
        /// exception. When this happens, all uncompleted tasks are cancelled
        /// and the exception is thrown.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list, each of which has completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAllOrFail<T>(IEnumerable<Func<T>> tasks)
        {
            ICollection<ICallable<T>> collection = tasks as ICollection<ICallable<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAllOrFail(tasks, count, false, TimeSpan.Zero, Call2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their results when all complete successfully or throws
        /// exception when any one fails.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Upon successful return, <see cref="ICancellable.IsDone"/> is 
        /// <c>true</c> and <see cref="IFuture{T}.Get()"/> is guaranteed
        /// to success for each element of the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// The method returns immediately when any one of the tasks throws 
        /// exception. When this happens, all uncompleted tasks are cancelled
        /// and the exception is thrown.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list, each of which has completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAllOrFail<T>(params Func<T>[] tasks)
        {
            return InvokeAllOrFail((IEnumerable<Func<T>>)tasks);
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their results when all complete successfully, or throws
        /// exception when <paramref name="durationToWait"/> expires or any 
        /// one task fails, whichever happens first.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Upon successful return, <see cref="ICancellable.IsDone"/> is 
        /// <c>true</c> and <see cref="IFuture{T}.Get()"/> is guaranteed
        /// to success for each element of the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// The method returns immediately when any one of the tasks throws 
        /// exception. When this happens, all uncompleted tasks are cancelled
        /// and the exception is thrown.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list. If the operation did not time out, each task will
        /// have completed. If it did time out, some of these tasks will
        /// not have completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, IEnumerable<ICallable<T>> tasks)
        {
            ICollection<ICallable<T>> collection = tasks as ICollection<ICallable<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAllOrFail(tasks, count, true, durationToWait, Callable2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their results when all complete successfully, or throws
        /// exception when <paramref name="durationToWait"/> expires or any 
        /// one task fails, whichever happens first.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Upon successful return, <see cref="ICancellable.IsDone"/> is 
        /// <c>true</c> and <see cref="IFuture{T}.Get()"/> is guaranteed
        /// to success for each element of the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// The method returns immediately when any one of the tasks throws 
        /// exception. When this happens, all uncompleted tasks are cancelled
        /// and the exception is thrown.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="ICallable{T}"/> objects.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list. If the operation did not time out, each task will
        /// have completed. If it did time out, some of these tasks will
        /// not have completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, params ICallable<T>[] tasks)
        {
            return InvokeAllOrFail(durationToWait, (IEnumerable<ICallable<T>>) tasks);
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their results when all complete successfully, or throws
        /// exception when <paramref name="durationToWait"/> expires or any 
        /// one task fails, whichever happens first.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Upon successful return, <see cref="ICancellable.IsDone"/> is 
        /// <c>true</c> and <see cref="IFuture{T}.Get()"/> is guaranteed
        /// to success for each element of the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// The method returns immediately when any one of the tasks throws 
        /// exception. When this happens, all uncompleted tasks are cancelled
        /// and the exception is thrown.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list. If the operation did not time out, each task will
        /// have completed. If it did time out, some of these tasks will
        /// not have completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, IEnumerable<Func<T>> tasks)
        {
            ICollection<ICallable<T>> collection = tasks as ICollection<ICallable<T>>;
            int count = collection == null ? 0 : collection.Count;
            return DoInvokeAllOrFail(tasks, count, true, durationToWait, Call2Future<T>());
        }

        /// <summary> 
        /// Executes the given <paramref name="tasks"/>, returning a 
        /// <see cref="IList{T}">list</see> of <see cref="IFuture{T}"/>s 
        /// holding their results when all complete successfully, or throws
        /// exception when <paramref name="durationToWait"/> expires or any 
        /// one task fails, whichever happens first.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Upon successful return, <see cref="ICancellable.IsDone"/> is 
        /// <c>true</c> and <see cref="IFuture{T}.Get()"/> is guaranteed
        /// to success for each element of the returned list.
        /// </para>
        /// <para>
        /// Note: 
        /// The method returns immediately when any one of the tasks throws 
        /// exception. When this happens, all uncompleted tasks are cancelled
        /// and the exception is thrown.
        /// The results of this method are undefined if the given
        /// enumerable is modified while this operation is in progress.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">
        /// The type of the result to be returned by <see cref="IFuture{T}"/>.
        /// </typeparam>
        /// <param name="tasks">
        /// The <see cref="IEnumerable{T}">enumeration</see> of 
        /// <see cref="Func{T}"/> delegates.
        /// </param>
        /// <param name="durationToWait">The time span to wait.</param> 
        /// <returns>
        /// A list of <see cref="IFuture{T}"/>s representing the tasks, in the 
        /// same sequential order as produced by the iterator for the given 
        /// task list. If the operation did not time out, each task will
        /// have completed. If it did time out, some of these tasks will
        /// not have completed.
        /// </returns>
        /// <exception cref="RejectedExecutionException">
        /// If the any of the <paramref name="tasks"/> cannot be accepted for 
        /// execution.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// If the <paramref name="tasks"/> is <c>null</c>.
        /// </exception>
        public virtual IList<IFuture<T>> InvokeAllOrFail<T>(TimeSpan durationToWait, params Func<T>[] tasks)
        {
            return InvokeAllOrFail(durationToWait, (IEnumerable<Func<T>>)tasks);
        }

        #endregion InvokeAllOrFail<T> Methods

        /// <summary> 
        /// Returns a <see cref="IRunnableFuture{T}"/> for the given 
        /// <paramref name="runnable"/> and default value <paramref name="result"/>.
        /// </summary>
        /// <param name="runnable">the runnable task being wrapped
        /// </param>
        /// <param name="result">the default value for the returned future
        /// </param>
        /// <returns>
        /// A <see cref="IRunnableFuture{T}"/> which, when run, will run the
        /// underlying runnable and which, as a <see cref="IFuture{T}"/>, will yield
        /// the given value as its result and provide for cancellation of
        /// the underlying task.
        /// </returns>
        protected internal virtual IRunnableFuture<T> NewTaskFor<T>(IRunnable runnable, T result)
        {
            return new FutureTask<T>(runnable, result);
        }

        /// <summary> 
        /// Returns a <see cref="IRunnableFuture{T}"/> for the given 
        /// <paramref name="action"/> and default value <paramref name="result"/>.
        /// </summary>
        /// <param name="action">the action being wrapped
        /// </param>
        /// <param name="result">the default value for the returned future
        /// </param>
        /// <returns>
        /// A <see cref="IRunnableFuture{T}"/> which, when run, will invoke the
        /// underlying action and which, as a <see cref="IFuture{T}"/>, will yield
        /// the given value as its result and provide for cancellation of
        /// the underlying task.
        /// </returns>
        protected internal virtual IRunnableFuture<T> NewTaskFor<T>(Action action, T result)
        {
            return new FutureTask<T>(action, result);
        }

        /// <summary> 
        /// Returns a <see cref="IRunnableFuture{T}"/> for the given 
        /// <paramref name="call"/> delegate.
        /// </summary>
        /// <param name="call">
        /// The <see cref="Func{T}"/> delegate being wrapped.
        /// </param>
        /// <returns>
        /// An <see cref="IRunnableFuture{T}"/> which when run will call the
        /// underlying <paramref name="call"/> delegate and which, as a 
        /// <see cref="IFuture{T}"/>, will yield the result of <c>call</c>as 
        /// its result and provide for cancellation of the underlying task.
        /// </returns>
        protected internal virtual IRunnableFuture<T> NewTaskFor<T>(Func<T> call)
        {
            return new FutureTask<T>(call);
        }

        /// <summary> 
        /// Returns a <see cref="IRunnableFuture{T}"/> for the given 
        /// <paramref name="callable"/> task.
        /// </summary>
        /// <param name="callable">The callable task being wrapped.</param>
        /// <returns>
        /// An <see cref="IRunnableFuture{T}"/> which when run will call the
        /// underlying <paramref name="callable"/> and which, as a 
        /// <see cref="IFuture{T}"/>, will yield the callable's result as its 
        /// result and provide for cancellation of the underlying task.
        /// </returns>
        protected internal virtual IRunnableFuture<T> NewTaskFor<T>(ICallable<T> callable)
        {
            return new FutureTask<T>(callable);
        }

        /// <summary>
        /// Create a new <see cref="IContextCarrier"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="IContextCarrier"/> instance or <c>null</c>
        /// if <see cref="ContextCarrierFactory"/> is not set.
        /// </returns>
        protected internal virtual IContextCarrier NewContextCarrier()
        {
            return _contextCarrierFactory == null ?
                null : _contextCarrierFactory.CreateContextCarrier();
        }

        /// <summary>
        /// Raises the <see cref="ThreadException"/> event.
        /// </summary>
        /// <param name="sender">
        /// The task that raised the exception.
        /// </param>
        /// <param name="exception">
        /// The exception object.
        /// </param>
        protected void OnThreadException(IRunnable sender, Exception exception)
        {
            var handler = ThreadException;
            if (handler != null) handler(sender, new ThreadExceptionEventArgs(exception));
            else throw exception.PreserveStackTrace();
        }

        private Converter<object, IRunnableFuture<T>> Callable2Future<T>()
        {
            return delegate(object callable) { return NewTaskFor((ICallable<T>)callable); };
        }

        private Converter<object, IRunnableFuture<T>> Call2Future<T>()
        {
            return delegate(object call) { return NewTaskFor((Func<T>)call); };
        }

        private IFuture<T> Submit<T>(IRunnableFuture<T> runnableFuture)
        {
            Execute(runnableFuture);
            return runnableFuture;
        }

        private T SetContextCarrier<T>(T task, IContextCarrier contextCarrier)
        {
            if (contextCarrier != null)
            {
                var contextCopying = task as IContextCopyingTask;
                if (contextCopying != null && contextCopying.ContextCarrier == null)
                {
                    contextCopying.ContextCarrier = contextCarrier;
                }
            }
            return task;
        }

        private T DoInvokeAny<T>(IEnumerable tasks, int count, bool timed, TimeSpan durationToWait, Converter<object, IRunnableFuture<T>> converter)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            List<IFuture<T>> futures = count > 0 ? new List<IFuture<T>>(count) : new List<IFuture<T>>();
			ExecutorCompletionService<T> ecs = new ExecutorCompletionService<T>(this);
			TimeSpan duration = durationToWait;

            // For efficiency, especially in executors with limited
            // parallelism, check to see if previously submitted tasks are
            // done before submitting more of them. This interleaving
            // plus the exception mechanics account for messiness of main
            // loop.

            try
            {
                // Record exceptions so that if we fail to obtain any
                // result, we can throw the last exception we got.
                ExecutionException ee = null;
				DateTime lastTime = (timed) ? DateTime.Now : new DateTime(0);
                IEnumerator it = tasks.GetEnumerator();
			    bool hasMoreTasks = it.MoveNext();
                if (!hasMoreTasks)
                    throw new ArgumentException("No tasks passed in.", "tasks");
                var contextCarrier = NewContextCarrier();
                futures.Add(ecs.DoSubmit(SetContextCarrier(converter(it.Current), contextCarrier)));
				int active = 1;

                for (;;)
                {
                    IFuture<T> f = ecs.Poll();
                    if (f == null)
                    {
                        if (hasMoreTasks && (hasMoreTasks = it.MoveNext()))
                        {
                            futures.Add(ecs.DoSubmit(SetContextCarrier(converter(it.Current), contextCarrier)));
                            ++active;
                        }
                        else if (active == 0)
                            break;
                        else if (timed)
                        {
                            f = ecs.Poll(duration);
                            if (f == null)
                                throw new TimeoutException();
                            // We need to recalculate wait time if Poll was interrupted
                            duration = duration.Subtract(DateTime.Now.Subtract(lastTime));
                            lastTime = DateTime.Now;
                        }
                        else
                            f = ecs.Take();
                    }
                    if (f != null)
                    {
                        --active;
                        try
                        {
                            return f.Get();
                        }
                        catch (ThreadInterruptedException e)
                        {
                            throw SystemExtensions.PreserveStackTrace(e);
                        }
                        catch (ExecutionException eex)
                        {
                            ee =  SystemExtensions.PreserveStackTrace(eex);
                        }
                        catch (Exception rex)
                        {
                            ee = new ExecutionException(rex);
                        }
                    }
                }

                if (ee == null)
                    ee = new ExecutionException();
                throw ee;
            }
            finally
            {
                foreach (IFuture<T> future in futures)
                {
                    future.Cancel(true);
                }
            }
        }

        private List<IFuture<T>> DoInvokeAll<T>(IEnumerable tasks, int count, Converter<object, IRunnableFuture<T>> converter)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
			List<IFuture<T>> futures = count > 0 ?  new List<IFuture<T>>(count) : new List<IFuture<T>>();
            bool done = false;
            try
            {
                var contextCarrier = NewContextCarrier();
                foreach (object task in tasks)
                {
                    IRunnableFuture<T> runnableFuture = SetContextCarrier(converter(task), contextCarrier);
                    futures.Add(runnableFuture);
                    Execute(runnableFuture);
                }

                foreach (IFuture<T> future in futures)
                {
                    if (!future.IsDone)
                    {
                        try
                        {
                            future.Get();
                        }
                        catch (CancellationException)
                        {
                        }
                        catch (ExecutionException)
                        {
                        }
                    }
                }
                done = true;
                return futures;
            }
            finally
            {
                if (!done)
                {
                    foreach (IFuture<T> future in futures)
                    {
                        future.Cancel(true);
                    }
                }
            }
        }

        private List<IFuture<T>> DoInvokeAll<T>(IEnumerable tasks, int count, TimeSpan durationToWait, Converter<object, IRunnableFuture<T>> converter)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            TimeSpan duration = durationToWait;
            List<IFuture<T>> futures = count > 0 ? new List<IFuture<T>>(count) : new List<IFuture<T>>();
            bool done = false;
            try
            {
                var contextCarrier = NewContextCarrier();
                foreach (object task in tasks)
                {
                    futures.Add(SetContextCarrier(converter(task), contextCarrier));
                }

                DateTime lastTime = DateTime.Now;

                // Interleave time checks and calls to execute in case
                // executor doesn't have any/much parallelism.
                foreach (IRunnable runnable in futures)
                {
                    Execute(runnable);

                    duration = duration.Subtract(DateTime.Now.Subtract(lastTime));
                    lastTime = DateTime.Now;
                    if (duration.Ticks <= 0)
                        return futures;
                }

                foreach (IFuture<T> future in futures)
                {
                    if (!future.IsDone)
                    {
                        if (duration.Ticks <= 0)
                            return futures;
                        try
                        {
                            future.Get(duration);
                        }
                        catch (CancellationException)
                        {
                        }
                        catch (ExecutionException)
                        {
                        }
                        catch (TimeoutException)
                        {
                            return futures;
                        }

                        duration = duration.Subtract(DateTime.Now.Subtract(lastTime));
                        lastTime = DateTime.Now;
                    }
                }
                done = true;
                return futures;
            }
            finally
            {
                if (!done)
                {
                    foreach (IFuture<T> future in futures)
                    {
                        future.Cancel(true);
                    }
                }
            }
        }

        private IList<IFuture<T>> DoInvokeAllOrFail<T>(IEnumerable tasks, int count, bool timed, TimeSpan durationToWait, Converter<object, IRunnableFuture<T>> converter)
        {
            if (tasks == null)
                throw new ArgumentNullException("tasks");
            List<IFuture<T>> futures = count > 0 ? new List<IFuture<T>>(count) : new List<IFuture<T>>();
            ExecutorCompletionService<T> ecs = new ExecutorCompletionService<T>(this);
            TimeSpan duration = durationToWait;

            try
            {
                DateTime lastTime = (timed) ? DateTime.Now : new DateTime(0);
                IEnumerator it = tasks.GetEnumerator();
                bool hasMoreTasks = it.MoveNext();
                if (!hasMoreTasks) return futures;
                var contextCarrier = NewContextCarrier();
                futures.Add(ecs.DoSubmit(SetContextCarrier(converter(it.Current), contextCarrier)));
                int active = 1;

                for (; ; )
                {
                    IFuture<T> f = ecs.Poll();
                    if (f == null)
                    {
                        if (hasMoreTasks && (hasMoreTasks = it.MoveNext()))
                        {
                            futures.Add(ecs.DoSubmit(SetContextCarrier(converter(it.Current), contextCarrier)));
                            ++active;
                        }
                        else if (active == 0)
                            break;
                        else if (timed)
                        {
                            f = ecs.Poll(duration);
                            if (f == null)
                                throw new TimeoutException();
                            // We need to recalculate wait time if Poll was interrupted
                            duration = duration.Subtract(DateTime.Now.Subtract(lastTime));
                            lastTime = DateTime.Now;
                        }
                        else
                            f = ecs.Take();
                    }
                    if (f != null)
                    {
                        --active;
                        try
                        {
                            f.Get();
                        }
                        catch (ThreadInterruptedException tie)
                        {
                            throw SystemExtensions.PreserveStackTrace(tie);
                        }
                        catch (ExecutionException eex)
                        {
                            throw SystemExtensions.PreserveStackTrace(eex);
                        }
                        catch (Exception e)
                        {
                            throw new ExecutionException(e);
                        }
                    }
                }
                return futures;
            }
            finally
            {
                foreach (IFuture<T> future in futures)
                {
                    if (!future.IsDone) future.Cancel(true);
                }
            }
        }

        /// <summary>
        /// If <paramref name="command"/> is <see cref="IContextCopyingTask"/>
        /// and the context carrier is already set, do nothing. Otherwise set
        /// a new context carrier.
        /// </summary>
        private IRunnable CaptureContext(IRunnable command)
        {
            var contextCopying = command as IContextCopyingTask;
            if (contextCopying == null)
            {
                var contextCarrier = NewContextCarrier();
                if (contextCarrier != null)
                {
                    command = new ContextCopyingRunnable(command, contextCarrier);
                }
            }
            else
            {
                if (contextCopying.ContextCarrier == null)
                {
                    contextCopying.ContextCarrier = NewContextCarrier();
                }
            }
            return command;
        }
    }
}