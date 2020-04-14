using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;

namespace Mud.Server.Tests
{
    [TestClass]
    public class ContainerTests
    {
        [TestMethod]
        public void Singleton_OneRegister_Test()
        {
            Container container = new Container();
            container.Register<IId, Class1>(Lifestyle.Singleton);

            var instance1 = container.GetInstance<IId>();
            var instance2 = container.GetInstance<IId>();

            Assert.AreEqual(instance1, instance2);
            Assert.AreEqual(instance1.Id, instance2.Id);
        }

        [TestMethod]
        public void Singleton_TwoRegistersWithDifferentInterfaces_Test()
        {
            Container container = new Container();
            container.Register<IId, Class2>(Lifestyle.Singleton);
            container.Register<IAdditional, Class2>(Lifestyle.Singleton);

            var instanceId = container.GetInstance<IId>();
            var instanceAdditional = container.GetInstance<IAdditional>();

            Assert.AreEqual(instanceId, instanceAdditional);
            Assert.AreEqual(instanceId.Id, ((IId)instanceAdditional).Id);
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
}
