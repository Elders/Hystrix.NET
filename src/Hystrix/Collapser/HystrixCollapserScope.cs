namespace Netflix.Hystrix
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The scope of request collapsing.
    /// </summary>
    public enum HystrixCollapserScope
    {
        /// <summary>
        /// Requests within the scope of a {@link HystrixRequestContext} will be collapsed.
        /// Typically this means that requests within a single user-request (ie. HTTP request) are collapsed. No interaction with other user requests. 1 queue per user request.
        /// </summary>
        Request,

        /// <summary>
        /// Requests from any thread (ie. all HTTP requests) within the JVM will be collapsed. 1 queue for entire app.
        /// </summary>
        Global,
    }
}
