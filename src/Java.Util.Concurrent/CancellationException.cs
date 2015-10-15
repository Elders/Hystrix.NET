using System;
using System.Runtime.Serialization;

namespace Java.Util.Concurrent
{
	/// <summary> 
	/// Exception indicating that the result of a value-producing task,
	/// such as a <see cref="FutureTask{T}"/>, cannot be retrieved because the task
	/// was cancelled.
	/// </summary>
	/// <author>Doug Lea</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class CancellationException : InvalidOperationException //JDK_1_6
	{
		#region Constructor (s) / Destructor

		/// <summary>Creates a new instance of the <see cref="Spring.Threading.Execution.CancellationException"/> class.</summary>.
		public CancellationException()
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Threading.Execution.CancellationException"/> class. with the specified message.
		/// </summary>
		/// <param name="message">A message about the exception.</param>
		public CancellationException(string message) : base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Threading.Execution.CancellationException"/> class with the specified message
		/// and root cause.
		/// </summary>
		/// <param name="message">A message about the exception.</param>
		/// <param name="rootCause">The root exception that is being wrapped.</param>
		public CancellationException(string message, Exception rootCause)
			: base(message, rootCause)
		{
		}

		/// <summary>
		/// Creates a new instance of the <see cref="Spring.Threading.Execution.CancellationException"/> class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected CancellationException(
			SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion
	}
}