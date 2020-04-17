using AutoBogus;
using AutoMapper;
using DeepEqual.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Container;

namespace Mud.Repository.Tests
{
    [TestClass]
    public class MongoMappingTests : MappingTestsBase
    {
        [TestMethod]
        public void Test_PlayerData_Success()
        {
            var original = AutoFaker.Generate<Domain.PlayerData>();

            var internalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.PlayerData, Mongo.Domain.PlayerData>(original);
            var externalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Mongo.Domain.PlayerData, Domain.PlayerData>(internalPlayerData);

            original.WithDeepEqual(externalPlayerData).Assert();
        }

        [TestMethod]
        [ExpectedException(typeof(DeepEqualException))]
        public void Test_PlayerData_Failed()
        {
            var original = AutoFaker.Generate<Domain.PlayerData>();

            var internalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.PlayerData, Mongo.Domain.PlayerData>(original);
            var externalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Mongo.Domain.PlayerData, Domain.PlayerData>(internalPlayerData);

            externalPlayerData.Name = "poeut";

            original.WithDeepEqual(externalPlayerData).Assert();
            Assert.Fail();
        }

        [TestMethod]
        public void Test_AdminData_Success()
        {
            var original = AutoFaker.Generate<Domain.AdminData>();

            var internalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.AdminData, Mongo.Domain.AdminData>(original);
            var externalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Mongo.Domain.AdminData, Domain.AdminData>(internalAdminData);

            original.WithDeepEqual(externalAdminData).Assert();
        }

        [TestMethod]
        [ExpectedException(typeof(DeepEqualException))]
        public void Test_AdminData_Failed()
        {
            var original = AutoFaker.Generate<Domain.AdminData>();

            var internalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.AdminData, Mongo.Domain.AdminData>(original);
            var externalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Mongo.Domain.AdminData, Domain.AdminData>(internalAdminData);

            externalAdminData.Name = "poeut";

            original.WithDeepEqual(externalAdminData).Assert();
        }
    }
}
