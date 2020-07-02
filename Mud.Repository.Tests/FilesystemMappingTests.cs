using System;
using System.Linq;
using System.Reflection;
using AutoBogus;
using AutoMapper;
using Bogus;
using DeepEqual.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Container;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;

namespace Mud.Repository.Tests
{
    [TestClass]
    public class FilesystemMappingTests : MappingTestsBase
    {
        [TestMethod]
        public void Test_PlayerData_Success()
        {
            var faker = new Faker<Domain.PlayerData>()
                .RuleForType<ICharacterFlags>(typeof(ICharacterFlags), x => new CharacterFlags(Rom24CharacterFlags.Flags.First()))
                .RuleForType<IItemFlags>(typeof(IItemFlags), x => new ItemFlags(Rom24ItemFlagValues.Flags.First()))
                .RuleForType<IWeaponFlags>(typeof(IWeaponFlags), x => new WeaponFlags(Rom24WeaponFlagValues.Flags.First()))
                .RuleForType<IIRVFlags>(typeof(IIRVFlags), x => new IRVFlags(Rom24IRVFlagValues.Flags.First()));
            var original = faker.Generate();

            var internalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.PlayerData, Filesystem.Domain.PlayerData>(original);
            var externalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Filesystem.Domain.PlayerData, Domain.PlayerData>(internalPlayerData);

            original.WithDeepEqual(externalPlayerData).Assert();
        }

        [TestMethod]
        [ExpectedException(typeof(DeepEqualException))]
        public void Test_PlayerData_Failed()
        {
            var faker = new Faker<Domain.PlayerData>()
                .RuleForType<ICharacterFlags>(typeof(ICharacterFlags), x => new CharacterFlags(Rom24CharacterFlags.Flags.First()))
                .RuleForType<IItemFlags>(typeof(IItemFlags), x => new ItemFlags(Rom24ItemFlagValues.Flags.First()))
                .RuleForType<IWeaponFlags>(typeof(IWeaponFlags), x => new WeaponFlags(Rom24WeaponFlagValues.Flags.First()))
                .RuleForType<IIRVFlags>(typeof(IIRVFlags), x => new IRVFlags(Rom24IRVFlagValues.Flags.First()));
            var original = faker.Generate();

            var internalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.PlayerData, Filesystem.Domain.PlayerData>(original);
            var externalPlayerData = DependencyContainer.Current.GetInstance<IMapper>().Map<Filesystem.Domain.PlayerData, Domain.PlayerData>(internalPlayerData);

            externalPlayerData.Name = AutoFaker.Generate<string>();

            original.WithDeepEqual(externalPlayerData).Assert();
            Assert.Fail();
        }

        [TestMethod]
        public void Test_AdminData_Success()
        {
            var faker = new Faker<Domain.AdminData>()
                .RuleForType<ICharacterFlags>(typeof(ICharacterFlags), x => new CharacterFlags(Rom24CharacterFlags.Flags.First()))
                .RuleForType<IItemFlags>(typeof(IItemFlags), x => new ItemFlags(Rom24ItemFlagValues.Flags.First()))
                .RuleForType<IWeaponFlags>(typeof(IWeaponFlags), x => new WeaponFlags(Rom24WeaponFlagValues.Flags.First()))
                .RuleForType<IIRVFlags>(typeof(IIRVFlags), x => new IRVFlags(Rom24IRVFlagValues.Flags.First()));
            var original = faker.Generate();

            var internalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.AdminData, Filesystem.Domain.AdminData>(original);
            var externalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Filesystem.Domain.AdminData, Domain.AdminData>(internalAdminData);

            original.WithDeepEqual(externalAdminData).Assert();
        }

        [TestMethod]
        [ExpectedException(typeof(DeepEqualException))]
        public void Test_AdminData_Failed()
        {
            var faker = new Faker<Domain.AdminData>()
                .RuleForType<ICharacterFlags>(typeof(ICharacterFlags), x => new CharacterFlags(Rom24CharacterFlags.Flags.First()))
                .RuleForType<IItemFlags>(typeof(IItemFlags), x => new ItemFlags(Rom24ItemFlagValues.Flags.First()))
                .RuleForType<IWeaponFlags>(typeof(IWeaponFlags), x => new WeaponFlags(Rom24WeaponFlagValues.Flags.First()))
                .RuleForType<IIRVFlags>(typeof(IIRVFlags), x => new IRVFlags(Rom24IRVFlagValues.Flags.First()));
            var original = faker.Generate();

            var internalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.AdminData, Filesystem.Domain.AdminData>(original);
            var externalAdminData = DependencyContainer.Current.GetInstance<IMapper>().Map<Filesystem.Domain.AdminData, Domain.AdminData>(internalAdminData);

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
                        Modifier = new WeaponFlags("Holy"),
                        Operator = Domain.AffectOperators.Assign,
                    },
                    new Domain.ItemFlagsAffectData
                    {
                        Modifier = new ItemFlags("Evil"),
                        Operator = Domain.AffectOperators.Or,
                    },
                    new Domain.CharacterSexAffectData
                    {
                        Value = Domain.Sex.Female,
                    },
                    new Domain.CharacterIRVAffectData
                    {
                        Location = Domain.IRVAffectLocations.Resistances,
                        Modifier = new IRVFlags("Cold"),
                        Operator = Domain.AffectOperators.Add,
                    },
                    new Domain.CharacterFlagsAffectData
                    {
                        Modifier = new CharacterFlags("Regeneration"),
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

            var internalAuraData = DependencyContainer.Current.GetInstance<IMapper>().Map<Domain.AuraData, Filesystem.Domain.AuraData>(original);
            var externalAuraData = DependencyContainer.Current.GetInstance<IMapper>().Map<Filesystem.Domain.AuraData, Domain.AuraData>(internalAuraData);

            original.WithDeepEqual(externalAuraData).Assert();
        }

        [Ignore]
        [TestMethod]
        public void Test_Concrete_Types()
        {
            var fileSystemDomainConcreteTypes = Assembly.GetAssembly(typeof(Filesystem.Domain.AdminData))
                .GetTypes()
                .Where(x => !x.IsAbstract && x.IsClass && x.Namespace == "Mud.Repository.Filesystem.Domain" && x.Name.Contains("Data"))
                .ToArray();
            var mudDomainConcreteTypes = Assembly.GetAssembly(typeof(Domain.AdminData))
                .GetTypes()
                .Where(x => !x.IsAbstract && x.IsClass && x.Namespace == "Mud.Domain" && x.Name.Contains("Data"))
                .ToArray();

            IMapper mapper = DependencyContainer.Current.GetInstance<IMapper>();

            // file system -> mud
            foreach (Type fileSystemDomainConcreteType in fileSystemDomainConcreteTypes)
            {
                Type mudDomainRelated = mudDomainConcreteTypes.FirstOrDefault(x => x.Name == fileSystemDomainConcreteType.Name);

                //var a = typeof(SpecimenFactory).GetMethods().Single(x => x.IsStatic && x.IsGenericMethod && x.Name == "Create" && x.GetParameters().Length == 1 && x.GetParameters().Single().ParameterType == typeof(ISpecimenBuilder)).MakeGenericMethod(fileSystemDomainConcreteType).Invoke(fixture, new[] { fixture });
                //var a = new SpecimenContext(fixture).Resolve(mudDomainRelated);
            }

            // mud -> file system
        }
    }
}
