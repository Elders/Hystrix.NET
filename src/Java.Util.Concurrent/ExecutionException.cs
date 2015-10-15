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
using System.Runtime.Serialization;

namespace Java.Util.Concurrent
{
	/// <summary> 
	/// Exception thrown when attempting to retrieve the result of a task
	/// that aborted by throwing an exception. 
	/// </summary>
	/// <seealso cref="IFuture{T}"/>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class ExecutionException : Exception //JDK_1_6
	{
		/// <summary> Constructs a <see cref="Spring.Threading.Execution.ExecutionException"/> with no detail message.</summary>
		public ExecutionException()
		{
		}

		/// <summary> Constructs a <see cref="Spring.Threading.Execution.ExecutionException"/> with the specified detail message.</summary>
		/// <param name="message">the detail message</param>
		public ExecutionException(String message) : base(message)
		{
		}

		/// <summary> Constructs a <see cref="Spring.Threading.Execution.ExecutionException"/> with the specified detail message and cause.</summary>
		/// <param name="message">the detail message</param>
		/// <param name="cause">the cause (which is saved for later retrieval by the</param>
		public ExecutionException(String message, Exception cause) : base(message, cause)
		{
		}

		/// <summary> 
		/// Constructs a <see cref="Spring.Threading.Execution.ExecutionException"/> with the specified cause.
		/// </summary>
		/// <param name="rootCause">The root exception that is being wrapped.</param>
		public ExecutionException(Exception rootCause) : base(rootCause.Message, rootCause)
		{
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Threading.Execution.ExecutionException"/> class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected ExecutionException(
			SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}