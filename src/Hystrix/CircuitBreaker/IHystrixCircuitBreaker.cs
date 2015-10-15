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

namespace Netflix.Hystrix.CircuitBreaker
{
    /// <summary>
    /// Circuit-breaker logic that is hooked into <see cref="HystrixCommand"/> execution and will stop allowing executions if failures have gone past the defined threshold.
    /// It will then allow single retries after a defined sleep window until the execution succeeds at which point it will close the circuit and allow executions again.
    /// </summary>
    public interface IHystrixCircuitBreaker
    {
        /// <summary>
        /// Every <see cref="HystrixCommand"/> request asks this if it is allowed to proceed or not.
        /// This takes into account the half-open logic which allows some requests through when determining if it should be closed again.
        /// </summary>
        /// <returns>True is the request is permitted, otherwise false.</returns>
        bool AllowRequest();

        /// <summary>
        /// Gets whether the circuit is currently open (tripped).
        /// </summary>
        /// <returns>True if the circuit is open, otherwise false.</returns>
        bool IsOpen();

        /// <summary>
        /// Invoked on successful executions from <see cref="HystrixCommand"/> as part of feedback mechanism when in a half-open state.
        /// </summary>
        void MarkSuccess();
    }
}
