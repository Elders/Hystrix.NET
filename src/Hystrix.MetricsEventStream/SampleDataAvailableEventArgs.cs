// Copyright 2013 Loránd Biró
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

namespace Hystrix.MetricsEventStream
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Event argument object to deliver JSON formatted metrics data.
    /// </summary>
    public class SampleDataAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleDataAvailableEventArgs"/> class.
        /// </summary>
        /// <param name="data">The JSON formatted metrics data.</param>
        public SampleDataAvailableEventArgs(IEnumerable<string> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            this.Data = data;
        }

        /// <summary>
        /// Gets the JSON formatted metrics data.
        /// </summary>
        public IEnumerable<string> Data { get; private set; }
    }
}
