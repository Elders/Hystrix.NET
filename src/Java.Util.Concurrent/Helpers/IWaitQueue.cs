#region License

/*
 * Copyright (C) 2002-2010 the original author or authors.
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

using System.Collections.Generic;
using System.Threading;

namespace Java.Util.Concurrent.Helpers
{
	/// <summary> 
	/// Interface for internal queue classes for semaphores, etc.
	/// Relies on implementations to actually implement queue mechanics.
    /// NOTE: this interface is NOT present in java.util.concurrent.
    /// </summary>	
	/// <author>Dawid Kurzyniec</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <changes>
	/// <list>
	/// <item>Renamed Insert to Enqueue</item>
	/// <item>Renamed Extract to Dequeue</item>
	/// </list>
	/// </changes>
	internal interface IWaitQueue // Was WaitQueue class in BACKPORT_3_1
	{
		int Length { get; }
		ICollection<Thread> WaitingThreads { get; }
	    bool HasNodes { get; }
	    bool IsWaiting(Thread thread);

	    void Enqueue(WaitNode w); // assumed not to block
	    WaitNode Dequeue(); // should return null if empty
        // In backport 3.1 but not used.
        //void PutBack(WaitNode w);
	}
}