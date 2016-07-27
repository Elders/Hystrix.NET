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

namespace Elders.Hystrix.NET.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;
    using Elders.Hystrix.NET.Util;

    /// <summary>
    /// RuntimeException that is thrown when a <see cref="HystrixCommand"/> fails and does not have a fallback.
    /// </summary>
    [Serializable]
    public class HystrixRuntimeException : Exception
    {
        public FailureType FailureCause { get; private set; }
        public Type CommandType { get; private set; }
        public Exception FallbackException { get; private set; }
        public StackTrace CallingThreadStackTrace { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRuntimeException"/> class with details about
        /// the cause and source of the error.
        /// </summary>
        /// <param name="failureCause"></param>
        /// <param name="commandType"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="fallbackException"></param>
        public HystrixRuntimeException(FailureType failureCause, Type commandType, string message, Exception innerException, Exception fallbackException)
            : base(message, innerException)
        {
            FailureCause = failureCause;
            CommandType = commandType;
            FallbackException = fallbackException;
            CallingThreadStackTrace = ExceptionThreadingUtility.GetCallingThreadStack();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRuntimeException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds
        /// the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains
        /// contextual information about the source or destination.</param>
        protected HystrixRuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
