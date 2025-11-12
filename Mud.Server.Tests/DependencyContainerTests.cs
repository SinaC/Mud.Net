using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;

namespace Mud.Server.Tests
{
    [TestClass]
    public class DependencyContainerTests
    {
        [TestMethod]
        public void Singleton_OneRegister_Test()
        {
            SimpleInjector.Container container = new SimpleInjector.Container();
            container.Register<IId, Class1>(Lifestyle.Singleton);

            var instance1 = container.GetInstance<IId>();
            var instance2 = container.GetInstance<IId>();

            Assert.AreSame(instance1, instance2);
            Assert.AreEqual(instance1.Id, instance2.Id);
        }

        [TestMethod]
        public void Singleton_TwoRegistersWithDifferentInterfaces_Test()
        {
            SimpleInjector.Container container = new SimpleInjector.Container();
            container.Register<IId, Class2>(Lifestyle.Singleton);
            container.Register<IAdditional, Class2>(Lifestyle.Singleton);

            var instanceId = container.GetInstance<IId>();
            var instanceAdditional = container.GetInstance<IAdditional>();

            Assert.AreSame<object>(instanceId, instanceAdditional);
            Assert.AreEqual(instanceId.Id, ((IId)instanceAdditional).Id);
        }

        [TestMethod]
        public void TestNoDefaultConstructor()
        {
            SimpleInjector.Container container = new SimpleInjector.Container();
            container.Register<IId, Class2>(Lifestyle.Singleton);
            container.Register<IAdditional, Class2>(Lifestyle.Singleton);
            container.Register<Class3>();

            var instance = container.GetInstance<Class3>();

            Assert.IsNotNull(instance);
            Assert.IsNotNull(instance.Id);
            Assert.IsNotNull(instance.Additional);
            Assert.IsInstanceOfType(instance.Id, typeof(Class2));
            Assert.IsInstanceOfType(instance.Additional, typeof(Class2));
            Assert.AreSame<object>(instance.Id, instance.Additional);
        }
    }

    public interface IId
    {
        Guid Id { get; }
    }

    public interface IAdditional
    {
    }

    internal class Class1 : IId
    {
        public Guid Id { get; }

        public Class1()
        {
            Id = Guid.NewGuid();
        }
    }

    internal class Class2 : IId, IAdditional
    {
        public Guid Id { get; }

        public Class2()
        {
            Id = Guid.NewGuid();
        }
    }

    internal class Class3
    {
        public IId Id { get; }
        public IAdditional Additional { get; }

        public Class3(IId id, IAdditional additional)
        {
            Id = id;
            Additional = additional;
        }
    }
}
