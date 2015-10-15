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

namespace Java.Util.Concurrent
{
    /// <summary> 
    /// An object that executes submitted <see cref="Spring.Threading.IRunnable"/> tasks
    /// or an <see cref="Action"/>.
    /// </summary>
    /// <remarks> 
    /// This interface provides a way of decoupling task submission from the
    /// mechanics of how each task will be run, including details of thread
    /// use, scheduling, etc.  An <see cref="Spring.Threading.IExecutor"/> is normally used
    /// instead of explicitly creating threads.
    /// <p/>
    /// However, the <see cref="Spring.Threading.IExecutor"/> interface does not strictly
    /// require that execution be asynchronous. In the simplest case, an
    /// executor can run the submitted task immediately in the caller's
    /// thread:
    /// 
    /// <code language="c#">
    /// class DirectExecutor : IExecutor {
    ///		public void Execute(IRunnable r) {
    ///			r.Run();
    /// 	}
    ///     public void Execute(Action task) {
    ///         task();
    ///     }
    /// }
    /// </code>
    /// <p/> 
    /// More typically, tasks are executed in some thread other
    /// than the caller's thread.  The executor below spawns a new thread
    /// for each task.
    /// 
    /// <code language="c#">
    /// class ThreadPerTaskExecutor : IExecutor {
    ///		public void Execute(IRunnable r) {
    /// 		new Thread(new ThreadStart(r.Run)).Start();
    /// 	}
    ///     public void Execute(Action task) {
    ///         new Thread(new ThreadStart(task)).Start();
    ///     }
    /// }
    /// </code>
    /// 
    /// Many <see cref="Spring.Threading.IExecutor"/> implementations impose some sort of
    /// limitation on how and when tasks are scheduled. 
    /// <p/>
    /// The <see cref="Spring.Threading.IExecutor"/> implementations provided in this package
    /// implement <see cref="Spring.Threading.Execution.IExecutorService"/>, which is a more extensive
    /// interface.  The <see cref="Spring.Threading.Execution.ThreadPoolExecutor"/> class provides an
    /// extensible thread pool implementation. The <see cref="Spring.Threading.Execution.Executors"/> class
    /// provides convenient factory methods for these Executors.
    /// </remarks>
    /// <author>Doug Lea</author>
    /// <author>Federico Spinazzi (.Net)</author>
    /// <author>Kenneth Xu</author>
    public interface IExecutor //JDK_1_6
    {
        /// <summary> 
        /// Executes the given command at some time in the future.
        /// </summary>
        /// <remarks>
        /// The command may execute in a new thread, in a pooled thread, or in the calling
        /// thread, at the discretion of the <see cref="Spring.Threading.IExecutor"/> implementation.
        /// </remarks>
        /// <param name="command">the runnable task</param>
        /// <exception cref="Spring.Threading.Execution.RejectedExecutionException">if the task cannot be accepted for execution.</exception>
        /// <exception cref="System.ArgumentNullException">if the command is null</exception>
        void Execute(IRunnable command);

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
        void Execute(Action action);
    }
}