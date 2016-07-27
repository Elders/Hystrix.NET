// Copyright 2012 Netflix, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Elders.Hystrix.NET
{
    /// <summary>
    /// Objects implementing the <see cref="ITimerListener"/> interface can be registered in <see cref="HystrixTimer"/> to call the <see cref="Tick()"/> method repeatedly.
    /// </summary>
    public interface ITimerListener
    {
        /// <summary>
        /// Gets how often this TimerListener should 'tick' defined in milliseconds.
        /// </summary>
        int IntervalTimeInMilliseconds { get; }

        /// <summary>
        /// <para>
        /// The 'tick' is called each time the interval occurs.
        /// </para>
        /// <para>
        /// This method should NOT block or do any work but instead fire its work asynchronously to perform on another thread otherwise it will prevent the Timer from functioning.
        /// </para>
        /// <para>
        /// This contract is used to keep this implementation single-threaded and simplistic.
        /// </para>
        /// <para>
        /// If you need a ThreadLocal set, you can store the state in the TimerListener, then when tick() is called, set the ThreadLocal to your desired value.
        /// </para>
        /// </summary>
        void Tick();
    }
}
