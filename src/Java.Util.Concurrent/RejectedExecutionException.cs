using System;
using System.Runtime.Serialization;

namespace Java.Util.Concurrent
{
	/// <summary> 
	/// Exception thrown by an <see cref="Spring.Threading.IExecutor"/> when a task cannot be
	/// accepted for execution.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio(.NET)</author>
	[Serializable]
	public class RejectedExecutionException : SystemException //JDK_1_6
	{
		/// <summary> 
		/// Constructs a <see cref="Spring.Threading.Execution.RejectedExecutionException"/> with no detail message.
		/// </summary>
		public RejectedExecutionException()
		{
		}

		/// <summary> 
		/// Constructs a <see cref="Spring.Threading.Execution.RejectedExecutionException"/> with the
		/// specified detail message.
		/// </summary>
		/// <param name="message">the detail message</param>
		public RejectedExecutionException(String message) : base(message)
		{
		}

		/// <summary> 
		/// Constructs a <see cref="Spring.Threading.Execution.RejectedExecutionException"/> with the
		/// specified detail message and cause.
		/// </summary>
		/// <param name="message">the detail message</param>
		/// <param name="innerException">the inner exception (which is saved for later retrieval by the)</param>
		public RejectedExecutionException(String message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary> 
		/// Constructs a <see cref="Spring.Threading.Execution.RejectedExecutionException"/> with the
		/// specified cause.
		/// </summary>
		/// <param name="innerException">the cause (which is saved for later retrieval by the)</param>
		public RejectedExecutionException(Exception innerException) : this(String.Empty, innerException)
		{
		}
		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Threading.Execution.RejectedExecutionException"/> class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected RejectedExecutionException(
			SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}	
	}
}