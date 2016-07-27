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
    using System;

    /// <summary>
    /// An immutable key to represent a <see cref="HystrixCommand"/> for monitoring, circuit-breakers,
    /// metrics publishing, caching and other such uses.
    /// Command keys are equal if their names are equal with ordinal string comparison.
    /// </summary>
    public class HystrixCommandKey : HystrixKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixCommandKey"/> class.
        /// </summary>
        /// <param name="name">The name of the command key.</param>
        public HystrixCommandKey(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixCommandKey"/> class based on the type of the command.
        /// This used to create default command keys for unnamed commands.
        /// </summary>
        /// <param name="commandType">The type of the command.</param>
        public HystrixCommandKey(Type commandType)
            : base(GetDefaultNameForCommandType(commandType))
        {
        }

        /// <summary>
        /// Converts a string to a <see cref="HystrixCommandKey"/> object.
        /// </summary>
        /// <param name="name">The name of the command key.</param>
        /// <returns>A <see cref="HystrixCommandKey"/> object constructed from the specified name.</returns>
        public static implicit operator HystrixCommandKey(string name)
        {
            return new HystrixCommandKey(name);
        }

        /// <summary>
        /// Gets the default name for a command type.
        /// </summary>
        /// <param name="commandType">The command type.</param>
        /// <returns>The default name for the command type.</returns>
        private static string GetDefaultNameForCommandType(Type commandType)
        {
            if (commandType == null)
            {
                throw new ArgumentNullException("commandType");
            }

            return commandType.Name;
        }
    }
}
