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

namespace Java.Util.Concurrent
{
    /// <summary>
    /// A task that can be canceled.
    /// </summary>
    /// <author>Kenneth Xu</author>
    public interface ICancellable //NET_ONLY
    {
        /// <summary> 
        /// Attempts to cancel execution of this task.  
        /// </summary>
        /// <remarks> 
        /// This attempt will fail if the task has already completed, already been cancelled,
        /// or could not be cancelled for some other reason. If successful,
        /// and this task has not started when <see cref="Cancel()"/> is called,
        /// this task should never run.  If the task has already started, the in-progress 
        /// tasks are allowed to complete.
        /// </remarks>
        /// <returns> <c>false</c> if the task could not be cancelled,
        /// typically because it has already completed normally;
        /// <c>true</c> otherwise
        /// </returns>
        /// 
        bool Cancel();

        /// <summary> 
        /// Attempts to cancel execution of this task.  
        /// </summary>
        /// <remarks> 
        /// This attempt will fail if the task has already completed, already been cancelled,
        /// or could not be cancelled for some other reason. If successful,
        /// and this task has not started when <see cref="Cancel()"/> is called,
        /// this task should never run.  If the task has already started,
        /// then the <paramref name="mayInterruptIfRunning"/> parameter determines
        /// whether the thread executing this task should be interrupted in
        /// an attempt to stop the task.
        /// </remarks>
        /// <param name="mayInterruptIfRunning">
        /// <c>true</c> if the thread executing this
        /// task should be interrupted; otherwise, in-progress tasks are allowed
        /// to complete
        /// </param>
        /// <returns>
        /// <c>false</c> if the task could not be cancelled,
        /// typically because it has already completed normally;
        /// <c>true</c> otherwise
        /// </returns>
        bool Cancel(bool mayInterruptIfRunning);

        /// <summary>
        /// Determines if this task was cancelled.
        /// </summary>
        /// <remarks> 
        /// Returns <c>true</c> if this task was cancelled before it completed
        /// normally.
        /// </remarks>
        /// <returns> <c>true</c>if task was cancelled before it completed
        /// </returns>
        bool IsCancelled { get; }

        /// <summary> 
        /// Returns <c>true</c> if this task completed.
        /// </summary>
        /// <remarks> 
        /// Completion may be due to normal termination, an exception, or
        /// cancellation -- in all of these cases, this method will return
        /// <c>true</c> if this task completed.
        /// </remarks>
        /// <returns> <c>true</c>if this task completed.</returns>
        bool IsDone { get; }
    }
}