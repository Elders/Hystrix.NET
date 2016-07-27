//namespace Elders.Hystrix.NET
//{
//    using System;
//    using System.Collections.Generic;
//    using Java.Util.Concurrent;
//    using Elders.Hystrix.NET.Strategy;
//    using Elders.Hystrix.NET.Strategy.Concurrency;
//    using slf4net;

//    /// <summary>
//    /// Collapse multiple requests into a single {@link HystrixCommand} execution based on a time window and optionally a max batch size.
//    /// This allows an object model to have multiple calls to the command that execute/queue many times in a short period (milliseconds) and have them all get batched into a single backend call.
//    /// Typically the time window is something like 10ms give or take.
//    /// NOTE: Do NOT retain any state within instances of this class.
//    /// It must be stateless or else it will be non-deterministic because most instances are discarded while some are retained and become the
//    /// "collapsers" for all the ones that are discarded.
//    /// </summary>
//    public abstract class HystrixCollapser
//    {
//        protected static readonly ILogger logger = LoggerFactory.GetLogger(typeof(HystrixCollapser));

//        private readonly ICollapserTimer timer;
//        private readonly IHystrixCollapserKey collapserKey;
//        private readonly IHystrixCollapserProperties properties;
//        private readonly HystrixCollapserScope scope;
//        private readonly IHystrixConcurrencyStrategy concurrencyStrategy;

//        private readonly HystrixRequestCache requestCache;

//        ///// <summary>
//        ///// Collapser with default {@link HystrixCollapserKey} derived from the implementing
//        ///// class name and scoped to {@link Scope#REQUEST} and default configuration.
//        ///// </summary>
//        //protected HystrixCollapser()
//        //    : this(HystrixCollapserSetter.WithCollapserKey(null).AndScope(HystrixCollapserScope.Request))
//        //{
//        //}

//        ///// <summary>
//        ///// Collapser scoped to {@link Scope#REQUEST} and default configuration.
//        ///// </summary>
//        ///// <param name="collapserKey">{@link HystrixCollapserKey} that identifies this collapser and provides the key used for retrieving properties, request caches, publishing metrics etc.</param>
//        //protected HystrixCollapser(IHystrixCollapserKey collapserKey)
//        //    : this(HystrixCollapserSetter.WithCollapserKey(collapserKey).AndScope(HystrixCollapserScope.Request))
//        //{
//        //}

//        ///// <summary>
//        ///// Construct a {@link HystrixCollapser} with defined {@link Setter} that allows
//        ///// injecting property and strategy overrides and other optional arguments.
//        ///// </summary>
//        ///// <param name="setter">Null values will result in the default being used.</param>
//        //protected HystrixCollapser(HystrixCollapserSetter setter)
//        //    : this(setter.CollapserKey, setter.Scope, new RealCollapserTimer(), setter.PropertiesSetter)
//        //{
//        //}

//        //private HystrixCollapser(IHystrixCollapserKey collapserKey, HystrixCollapserScope scope, ICollapserTimer timer, HystrixCollapserPropertiesSetter propertiesBuilder)
//        //{
//        //    /* strategy: ConcurrencyStrategy */
//        //    this.concurrencyStrategy = HystrixPlugins.Instance.ConcurrencyStrategy;

//        //    this.timer = timer;
//        //    this.scope = scope;
//        //    if (collapserKey == null || String.IsNullOrEmpty(collapserKey.Name))
//        //    {
//        //        string defaultKeyName = GetDefaultNameFromClass(GetType());
//        //        this.collapserKey = HystrixCollapserKeyFactory.AsKey(defaultKeyName);
//        //    }
//        //    else
//        //    {
//        //        this.collapserKey = collapserKey;
//        //    }
//        //    this.requestCache = HystrixRequestCache.GetInstance(this.collapserKey, this.concurrencyStrategy);
//        //    this.properties = HystrixPropertiesFactory.GetCollapserProperties(this.collapserKey, propertiesBuilder);
//        //}

//        /// <summary>
//        /// Key of the {@link HystrixCollapser} used for properties, metrics, caches, reporting etc.
//        /// </summary>
//        public IHystrixCollapserKey CollapserKey { get { return this.collapserKey; } }

//        /// <summary>
//        /// Scope of collapsing.
//        /// </summary>
//        public HystrixCollapserScope Scope { get { return this.scope; } }

//        internal static void Reset()
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public abstract class HystrixCollapser<BatchReturnType, ResponseType, RequestArgumentType> : HystrixCollapser, IHystrixExecutable<ResponseType>
//    {
//        public IHystrixCollapserKey CollapserKey { get; private set; }
//        public HystrixCollapserScope Scope { get; private set; }

//        public abstract RequestArgumentType RequestArgument { get; }

//        protected HystrixCollapser()
//        {
//            throw new NotImplementedException();
//        }
//        protected HystrixCollapser(IHystrixCollapserKey collapserKey)
//        {
//            throw new NotImplementedException();
//        }
//        protected HystrixCollapser(HystrixCollapserSetter setter)
//        {
//            throw new NotImplementedException();
//        }

//        protected abstract HystrixCommand<BatchReturnType> CreateCommand(IEnumerable<CollapsedRequest<ResponseType, RequestArgumentType>> requests);
//        protected IEnumerable<IEnumerable<CollapsedRequest<ResponseType, RequestArgumentType>>> ShardRequests(IEnumerable<CollapsedRequest<ResponseType, RequestArgumentType>> requests)
//        {
//            throw new NotImplementedException();
//        }
//        protected abstract void MapResponseToRequests(BatchReturnType batchResponse, IEnumerable<CollapsedRequest<ResponseType, RequestArgumentType>> requests);
//        public ResponseType Execute()
//        {
//            throw new NotImplementedException();
//        }
//        public IFuture<ResponseType> Queue()
//        {
//            throw new NotImplementedException();
//        }

//        public interface CollapsedRequest<ResponseType, RequestArgumentType>
//        {
//            RequestArgumentType Argument { get; }

//            void SetResponse(ResponseType response);
//            void SetException(System.Exception exception);
//        }

//        protected virtual String GetCacheKey()
//        {
//            return null;
//        }

//    }
//}
