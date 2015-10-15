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

namespace Netflix.Hystrix.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception representing an error with provided arguments or state rather than an execution failure.
    /// Unlike all other exceptions thrown by a <see cref="HystrixCommand"/> this will not trigger fallback,
    /// not count against failure metrics and thus not trigger the circuit breaker.
    /// NOTE: This should only be used when an error is due to user input such as <see cref="ArgumentException"/>
    /// otherwise it defeats the purpose of fault-tolerance and fallback behavior.
    /// </summary>
    [Serializable]
    public class HystrixBadRequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixBadRequestException"/> class with a specified
        /// error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public HystrixBadRequestException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixBadRequestException"/> class with a specified
        /// error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception specified.</param>
        public HystrixBadRequestException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixBadRequestException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds
        /// the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains
        /// contextual information about the source or destination.</param>
        protected HystrixBadRequestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
