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
        [TestInitialize]
        public void TestInitialize()
        {
            AutoFaker.Configure(builder =>
            {
                builder
                  //.WithBinder(new AutoBogus.Moq.MoqBinder())
                  .WithRepeatCount(5)    // Configures the number of items in a collection
                  .WithRecursiveDepth(10); // Configures how deep nested types should recurse
            });
        }

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

            externalPlayerData.Name = AutoFaker.Generate<string>();

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

            externalAdminData.Name = AutoFaker.Generate<string>();

            original.WithDeepEqual(externalAdminData).Assert();
        }

        // AffectDataBase is abstract, so AuraData.Affects will always be populated with null
        [TestMethod]
        public void Test_Affects_Success()
        {
            Domain.AuraData original = new Domain.AuraData
            {
                Affects = new Domain.AffectDataBase[] 
                {
                    new Domain.ItemWeaponFlagsAffectData 
                    {
                        Modifier = Domain.WeaponFlags.Holy,
                        Operator = Domain.AffectOperators.Assign,
                    },
                    new Domain.ItemFlagsAffectData
                    {
                        Modifier = Domain.ItemFlags.Evil,
                        Operator = Domain.AffectOperators.Or,
                    },
                    new Domain.CharacterSexAffectData
                    {
                        Value = Domain.Sex.Female,
                    },
                    new Domain.CharacterIRVAffectData
                    {
                        Location = Domain.IRVAffectLocations.Resistances,
                        Modifier = Domain.IRVFlags.Cold,
                        Operator = Domain.AffectOperators.Add,
                    },
                    new Domain.CharacterFlagsAffectData
                    {
                        Modifier = Domain.CharacterFlags.Regeneration,
                        Operator = Domain.AffectOperators.Nor,
                    },
                    new Domain.CharacterAttributeAffectData
                    {
                        Location = Domain.CharacterAttributeAffectLocations.SavingThrow,
                        Modifier = -5,
                        Operator = Domain.AffectOperators.Assign,
                    },
                }
            };

            var internalAuraData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.AuraData, Filesystem.DataContracts.AuraData>(original);
            var externalAuraData = DependencyContainer.Current.GetInstance<IMapper>().Map<Filesystem.DataContracts.AuraData, Domain.AuraData>(internalAuraData);

            original.WithDeepEqual(externalAuraData).Assert();
        }
    }
}
