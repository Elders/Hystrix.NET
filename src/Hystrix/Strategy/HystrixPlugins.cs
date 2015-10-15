namespace Netflix.Hystrix.Strategy
{
    using System;
    using System.Configuration;
    using System.Linq;
    using Java.Util.Concurrent.Atomic;
    using Netflix.Hystrix.Strategy.Concurrency;
    using Netflix.Hystrix.Strategy.EventNotifier;
    using Netflix.Hystrix.Strategy.ExecutionHook;
    using Netflix.Hystrix.Strategy.Metrics;
    using Netflix.Hystrix.Strategy.Properties;

    public class HystrixPlugins
    {
        private static readonly HystrixPlugins instance = new HystrixPlugins();
        public static HystrixPlugins Instance { get { return instance; } }

        private readonly AtomicReference<IHystrixEventNotifier> eventNotifier = new AtomicReference<IHystrixEventNotifier>();
        private readonly AtomicReference<IHystrixConcurrencyStrategy> concurrencyStrategy = new AtomicReference<IHystrixConcurrencyStrategy>();
        private readonly AtomicReference<IHystrixMetricsPublisher> metricsPublisher = new AtomicReference<IHystrixMetricsPublisher>();
        private readonly AtomicReference<IHystrixPropertiesStrategy> propertyStrategy = new AtomicReference<IHystrixPropertiesStrategy>();
        private readonly AtomicReference<IHystrixCommandExecutionHook> commandExecutionHook = new AtomicReference<IHystrixCommandExecutionHook>();

        private HystrixPlugins()
        {
        }

        public IHystrixEventNotifier EventNotifier
        {
            get
            {
                if (this.eventNotifier.Value == null)
                {
                    this.eventNotifier.CompareAndSet(null, GetPluginImplementationViaConfiguration<IHystrixEventNotifier>() ?? HystrixEventNotifierDefault.Instance);
                }

                return this.eventNotifier.Value;
            }
        }
        public IHystrixConcurrencyStrategy ConcurrencyStrategy
        {
            get
            {
                if (this.concurrencyStrategy.Value == null)
                {
                    this.concurrencyStrategy.CompareAndSet(null, GetPluginImplementationViaConfiguration<IHystrixConcurrencyStrategy>() ?? HystrixConcurrencyStrategyDefault.Instance);
                }

                return this.concurrencyStrategy.Value;
            }
        }
        public IHystrixMetricsPublisher MetricsPublisher
        {
            get
            {
                if (this.metricsPublisher.Value == null)
                {
                    this.metricsPublisher.CompareAndSet(null, GetPluginImplementationViaConfiguration<IHystrixMetricsPublisher>() ?? HystrixMetricsPublisherDefault.Instance);
                }

                return this.metricsPublisher.Value;
            }
        }
        public IHystrixPropertiesStrategy PropertiesStrategy
        {
            get
            {
                if (this.propertyStrategy.Value == null)
                {
                    this.propertyStrategy.CompareAndSet(null, GetPluginImplementationViaConfiguration<IHystrixPropertiesStrategy>() ?? HystrixPropertiesStrategyDefault.Instance);
                }

                return this.propertyStrategy.Value;
            }
        }
        public IHystrixCommandExecutionHook CommandExecutionHook
        {
            get
            {
                if (this.commandExecutionHook.Value == null)
                {
                    this.commandExecutionHook.CompareAndSet(null, GetPluginImplementationViaConfiguration<IHystrixCommandExecutionHook>() ?? HystrixCommandExecutionHookDefault.Instance);
                }

                return this.commandExecutionHook.Value;
            }
        }

        public void RegisterEventNotifier(IHystrixEventNotifier implementation)
        {
            if (!this.eventNotifier.CompareAndSet(null, implementation))
            {
                throw new InvalidOperationException("Another strategy was alread registered.");
            }
        }
        public void RegisterConcurrencyStrategy(IHystrixConcurrencyStrategy implementation)
        {
            if (!this.concurrencyStrategy.CompareAndSet(null, implementation))
            {
                throw new InvalidOperationException("Another strategy was alread registered.");
            }
        }
        public void RegisterMetricsPublisher(IHystrixMetricsPublisher implementation)
        {
            if (!this.metricsPublisher.CompareAndSet(null, implementation))
            {
                throw new InvalidOperationException("Another strategy was alread registered.");
            }
        }
        public void RegisterPropertiesStrategy(IHystrixPropertiesStrategy implementation)
        {
            if (!this.propertyStrategy.CompareAndSet(null, implementation))
            {
                throw new InvalidOperationException("Another strategy was alread registered.");
            }
        }
        public void RegisterCommandExecutionHook(IHystrixCommandExecutionHook implementation)
        {
            if (!this.commandExecutionHook.CompareAndSet(null, implementation))
            {
                throw new InvalidOperationException("Another strategy was alread registered.");
            }
        }

        private static T GetPluginImplementationViaConfiguration<T>()
        {
            return (T)GetPluginImplementationViaConfiguration(typeof(T));
        }
        private static object GetPluginImplementationViaConfiguration(Type pluginType)
        {
            string pluginTypeName = pluginType.Name;
            string implementationTypeName = ConfigurationManager.AppSettings["Hystrix.Plugin." + pluginTypeName + ".Implementation"];
            if (String.IsNullOrEmpty(implementationTypeName))
                return null;

            Type implementationType;
            try
            {
                implementationType = Type.GetType(implementationTypeName, true);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("{0} implementation class not found: {1}", pluginType, implementationTypeName), e);
            }

            if (!implementationType.IsSubclassOf(pluginType) && !implementationType.GetInterfaces().Contains(pluginType))
            {
                throw new Exception(String.Format("{0} implementation is not an instance of {0}: {1}", pluginTypeName, implementationTypeName));
            }

            try
            {
                return Activator.CreateInstance(implementationType);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("{0} implementation not able to be instantiated: {1}", pluginType, implementationTypeName), e);
            }
        }
    }
}
