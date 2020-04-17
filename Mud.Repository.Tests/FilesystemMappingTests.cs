using AutoBogus;
using AutoMapper;
using DeepEqual.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Container;

namespace Mud.Repository.Tests
{
    [TestClass]
    public class FilesystemMappingTests : MappingTestsBase
    {
        [TestMethod]
        public void Test_PlayerData_Success()
        {
            var original = AutoFaker.Generate<Domain.PlayerData>();

            var internalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.PlayerData, Filesystem.DataContracts.PlayerData>(original);
            var externalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Filesystem.DataContracts.PlayerData, Domain.PlayerData>(internalPlayerData);

            original.WithDeepEqual(externalPlayerData).Assert();
        }

        [TestMethod]
        [ExpectedException(typeof(DeepEqualException))]
        public void Test_PlayerData_Failed()
        {
            var original = AutoFaker.Generate<Domain.PlayerData>();

            var internalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.PlayerData, Filesystem.DataContracts.PlayerData>(original);
            var externalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Filesystem.DataContracts.PlayerData, Domain.PlayerData>(internalPlayerData);

            externalPlayerData.Name = "poeut";

            original.WithDeepEqual(externalPlayerData).Assert();
            Assert.Fail();
        }

        [TestMethod]
        public void Test_AdminData_Success()
        {
            var original = AutoFaker.Generate<Domain.AdminData>();

            var internalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.AdminData, Filesystem.DataContracts.AdminData>(original);
            var externalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Filesystem.DataContracts.AdminData, Domain.AdminData>(internalAdminData);

            original.WithDeepEqual(externalAdminData).Assert();
        }

        [TestMethod]
        [ExpectedException(typeof(DeepEqualException))]
        public void Test_AdminData_Failed()
        {
            var original = AutoFaker.Generate<Domain.AdminData>();

            var internalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.AdminData, Filesystem.DataContracts.AdminData>(original);
            var externalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Filesystem.DataContracts.AdminData, Domain.AdminData>(internalAdminData);

            externalAdminData.Name = "poeut";

            original.WithDeepEqual(externalAdminData).Assert();
        }
    }
}
