namespace Hystrix.Test.Strategy.Properties
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Netflix.Hystrix;

    [TestClass]
    public class HystrixPropertyTest
    {
        [TestMethod]
        public void PropertyFactory_Nested1()
        {
            IHystrixProperty<string> a = HystrixPropertyFactory.AsProperty("a");
            Assert.AreEqual("a", a.Get());

            IHystrixProperty<string> aWithDefault = HystrixPropertyFactory.AsProperty(a, "b");
            Assert.AreEqual("a", aWithDefault.Get());
        }


        [TestMethod]
        public void PropertyFactory_Nested2()
        {
            IHystrixProperty<string> nullValue = HystrixPropertyFactory.NullProperty<string>();

            IHystrixProperty<string> withDefault = HystrixPropertyFactory.AsProperty(nullValue, "b");
            Assert.AreEqual("b", withDefault.Get());
        }

        [TestMethod]
        public void PropertyFactory_Nested3()
        {
            IHystrixProperty<string> nullValue = HystrixPropertyFactory.NullProperty<string>();
            IHystrixProperty<string> a = HystrixPropertyFactory.AsProperty(nullValue, "a");

            IHystrixProperty<string> withDefault = HystrixPropertyFactory.AsProperty(a, "b");
            Assert.AreEqual("a", withDefault.Get());
        }

        [TestMethod]
        public void PropertyFactory_Nested4()
        {
            IHystrixProperty<string> nullValue = HystrixPropertyFactory.NullProperty<string>();
            IHystrixProperty<string> a = HystrixPropertyFactory.AsProperty(nullValue, (string)null);

            IHystrixProperty<string> withDefault = HystrixPropertyFactory.AsProperty(a, "b");
            Assert.AreEqual("b", withDefault.Get());
        }

        [TestMethod]
        public void PropertyFactory_Nested5()
        {
            IHystrixProperty<string> nullValue = HystrixPropertyFactory.NullProperty<string>();
            IHystrixProperty<string> a = HystrixPropertyFactory.AsProperty(nullValue, (string)null);

            IHystrixProperty<string> withDefault = HystrixPropertyFactory.AsProperty(a, HystrixPropertyFactory.AsProperty("b"));
            Assert.AreEqual("b", withDefault.Get());
        }

        [TestMethod]
        public void PropertyFactory_Series1()
        {
            IHystrixProperty<string> nullValue = HystrixPropertyFactory.NullProperty<string>();
            IHystrixProperty<string> a = HystrixPropertyFactory.AsProperty(nullValue, (string)null);

            IHystrixProperty<string> withDefault = HystrixPropertyFactory.AsProperty(a, nullValue, nullValue, HystrixPropertyFactory.AsProperty("b"));
            Assert.AreEqual("b", withDefault.Get());
        }

        [TestMethod]
        public void PropertyFactory_Series2()
        {
            IHystrixProperty<string> nullValue = HystrixPropertyFactory.NullProperty<string>();
            IHystrixProperty<string> a = HystrixPropertyFactory.AsProperty(nullValue, (string)null);

            IHystrixProperty<string> withDefault = HystrixPropertyFactory.AsProperty(a, nullValue, HystrixPropertyFactory.AsProperty("b"), nullValue, HystrixPropertyFactory.AsProperty("c"));
            Assert.AreEqual("b", withDefault.Get());
        }
    }
}
