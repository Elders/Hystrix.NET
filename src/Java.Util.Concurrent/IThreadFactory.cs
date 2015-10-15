#region License
/*
* Copyright ?2002-2005 the original author or authors.
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
using System.Threading;

namespace Java.Util.Concurrent
{
	
	/// <summary> 
	/// An object that creates new threads on demand.  
	/// </summary>
	/// <remarks> 
	/// Using thread factories removes hardwiring of calls to new Thread,
	/// enabling applications to use special thread subclasses, priorities, etc.
	/// <p/>
	/// The simplest implementation of this interface is just:
	/// <code>
	/// class SimpleThreadFactory : IThreadFactory {
	///		public Thread NewThread(IRunnable r) {
	///			return new Thread(new ThreadStart(r.Run));
	///		}
	/// }
	/// </code>
	/// 
	/// The <see cref="Executors.NewDefaultThreadFactory"/> method provides a more
	/// useful simple implementation, that sets the created thread context
	/// to known values before returning it.
	/// </remarks>
	/// <author>Doug Lea</author>
    /// <author>Federico Spinazzi (.Net)</author>
    /// <author>Griffin Caprio (.Net)</author>
    public interface IThreadFactory //JDK_1_6
    {
		/// <summary> 
		/// Constructs a new <see cref="System.Threading.Thread"/>.  
		/// </summary>
		/// <remarks> 
		/// Implementations may also initialize
		/// priority, name, daemon status, thread state, etc.
		/// </remarks>
		/// <param name="runnable">
		/// a runnable to be executed by new thread instance
		/// </param>
		/// <returns>constructed thread</returns>
        Thread NewThread(IRunnable runnable);
    }
}