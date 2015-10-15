using System;
using System.Collections.Generic;
using System.Text;

namespace Java.Util.Concurrent
{
    internal interface IRecommendParallelism // NET_ONLY
    {
        int MaxParallelism { get; }
    }
}
