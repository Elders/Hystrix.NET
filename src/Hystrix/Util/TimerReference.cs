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

namespace Netflix.Hystrix.Util
{
    using System.Threading;

    /// <summary>
    /// A reference to the started <see cref="HystrixTimer"/> so it can be stopped with the <see cref="Clear()"/> method.
    /// </summary>
    internal class TimerReference
    {
        /// <summary>
        /// The internal timer.
        /// </summary>
        private readonly Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerReference"/> class.
        /// </summary>
        /// <param name="listener">The listener to call repeatedly.</param>
        /// <param name="timer">The running background timer.</param>
        public TimerReference(ITimerListener listener, Timer timer)
        {
            this.timer = timer;
            this.Listener = listener;
        }

        /// <summary>
        /// Gets the registered listener.
        /// </summary>
        public ITimerListener Listener { get; private set; }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        public void Clear()
        {
            this.Listener = null;
            this.timer.Dispose();
        }
    }
}
