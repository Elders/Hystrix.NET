namespace Netflix.Hystrix.Strategy.Concurrency
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class HystrixRequestVariableCacheKey
    {
        private readonly HystrixRequestVariableHolder rvHolder;
        private readonly IHystrixConcurrencyStrategy concurrencyStrategy;

        public HystrixRequestVariableCacheKey(HystrixRequestVariableHolder rvHolder, IHystrixConcurrencyStrategy concurrencyStrategy)
        {
            this.rvHolder = rvHolder;
            this.concurrencyStrategy = concurrencyStrategy;
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((concurrencyStrategy == null) ? 0 : concurrencyStrategy.GetHashCode());
            result = prime * result + ((rvHolder == null) ? 0 : rvHolder.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            HystrixRequestVariableCacheKey other = (HystrixRequestVariableCacheKey)obj;
            if (concurrencyStrategy == null)
            {
                if (other.concurrencyStrategy != null)
                    return false;
            }
            else if (!concurrencyStrategy.Equals(other.concurrencyStrategy))
                return false;
            if (rvHolder == null)
            {
                if (other.rvHolder != null)
                    return false;
            }
            else if (!rvHolder.Equals(other.rvHolder))
                return false;
            return true;
        }
    }
}
