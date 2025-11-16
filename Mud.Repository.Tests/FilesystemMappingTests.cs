using AutoBogus;
using AutoMapper;
using Bogus;
using DeepEqual.Syntax;
using Microsoft.Extensions.DependencyInjection;
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
                .RuleForType<ICharacterFlags>(typeof(ICharacterFlags), x => new CharacterFlags(_serviceProvider, Rom24CharacterFlags.Flags.First()))
                .RuleForType<IItemFlags>(typeof(IItemFlags), x => new ItemFlags(_serviceProvider, Rom24ItemFlagValues.Flags.First()))
                .RuleForType<IWeaponFlags>(typeof(IWeaponFlags), x => new WeaponFlags(_serviceProvider, Rom24WeaponFlagValues.Flags.First()))
                .RuleForType<IIRVFlags>(typeof(IIRVFlags), x => new IRVFlags(_serviceProvider, Rom24IRVFlagValues.Flags.First()));
            var original = faker.Generate();

            var internalPlayerData = _serviceProvider.GetService<IMapper>()?.Map<Domain.PlayerData, Filesystem.Domain.PlayerData>(original);
            var externalPlayerData = _serviceProvider.GetService<IMapper>()?.Map<Filesystem.Domain.PlayerData, Domain.PlayerData>(internalPlayerData);

            original.WithDeepEqual(externalPlayerData).Assert();
        }

        [TestMethod]
        public void Test_PlayerData_Failed()
        {
            var faker = new Faker<Domain.PlayerData>()
                .RuleForType<ICharacterFlags>(typeof(ICharacterFlags), x => new CharacterFlags(_serviceProvider, Rom24CharacterFlags.Flags.First()))
                .RuleForType<IItemFlags>(typeof(IItemFlags), x => new ItemFlags(_serviceProvider, Rom24ItemFlagValues.Flags.First()))
                .RuleForType<IWeaponFlags>(typeof(IWeaponFlags), x => new WeaponFlags(_serviceProvider, Rom24WeaponFlagValues.Flags.First()))
                .RuleForType<IIRVFlags>(typeof(IIRVFlags), x => new IRVFlags(_serviceProvider, Rom24IRVFlagValues.Flags.First()));
            var original = faker.Generate();

            var internalPlayerData = _serviceProvider.GetService<IMapper>()?.Map<Domain.PlayerData, Filesystem.Domain.PlayerData>(original);
            var externalPlayerData = _serviceProvider.GetService<IMapper>()?.Map<Filesystem.Domain.PlayerData, Domain.PlayerData>(internalPlayerData);

            externalPlayerData.Name = AutoFaker.Generate<string>();

            Assert.Throws<DeepEqualException>(() => original.WithDeepEqual(externalPlayerData).Assert());
        }

        [TestMethod]
        public void Test_AdminData_Success()
        {
            var faker = new Faker<Domain.AdminData>()
                .RuleForType<ICharacterFlags>(typeof(ICharacterFlags), x => new CharacterFlags(_serviceProvider, Rom24CharacterFlags.Flags.First()))
                .RuleForType<IItemFlags>(typeof(IItemFlags), x => new ItemFlags(_serviceProvider, Rom24ItemFlagValues.Flags.First()))
                .RuleForType<IWeaponFlags>(typeof(IWeaponFlags), x => new WeaponFlags(_serviceProvider, Rom24WeaponFlagValues.Flags.First()))
                .RuleForType<IIRVFlags>(typeof(IIRVFlags), x => new IRVFlags(_serviceProvider, Rom24IRVFlagValues.Flags.First()));
            var original = faker.Generate();

            var internalAdminData = _serviceProvider.GetService<IMapper>()?.Map<Domain.AdminData, Filesystem.Domain.AdminData>(original);
            var externalAdminData = _serviceProvider.GetService<IMapper>()?.Map<Filesystem.Domain.AdminData, Domain.AdminData>(internalAdminData);

            original.WithDeepEqual(externalAdminData).Assert();
        }

        [TestMethod]
        public void Test_AdminData_Failed()
        {
            var faker = new Faker<Domain.AdminData>()
                .RuleForType<ICharacterFlags>(typeof(ICharacterFlags), x => new CharacterFlags(_serviceProvider, Rom24CharacterFlags.Flags.First()))
                .RuleForType<IItemFlags>(typeof(IItemFlags), x => new ItemFlags(_serviceProvider, Rom24ItemFlagValues.Flags.First()))
                .RuleForType<IWeaponFlags>(typeof(IWeaponFlags), x => new WeaponFlags(_serviceProvider, Rom24WeaponFlagValues.Flags.First()))
                .RuleForType<IIRVFlags>(typeof(IIRVFlags), x => new IRVFlags(_serviceProvider, Rom24IRVFlagValues.Flags.First()));
            var original = faker.Generate();

            var internalAdminData = _serviceProvider.GetService<IMapper>()?.Map<Domain.AdminData, Filesystem.Domain.AdminData>(original);
            var externalAdminData = _serviceProvider.GetService<IMapper>()?.Map<Filesystem.Domain.AdminData, Domain.AdminData>(internalAdminData);

            externalAdminData.Name = AutoFaker.Generate<string>();

            Assert.Throws<DeepEqualException>(() => original.WithDeepEqual(externalAdminData).Assert());
        }

        // AffectDataBase is abstract, so AuraData.Affects will always be populated with null
        [TestMethod]
        public void Test_Affects_Success()
        {
            Domain.AuraData original = new()
            {
                AbilityName = "Test Aura",
                Level = 1,
                PulseLeft = 1,
                AuraFlags = Domain.AuraFlags.None,
                Affects =
                [
                    new Domain.ItemWeaponFlagsAffectData 
                    {
                        Modifier = new WeaponFlags(_serviceProvider,"Holy"),
                        Operator = Domain.AffectOperators.Assign,
                    },
                    new Domain.ItemFlagsAffectData
                    {
                        Modifier = new ItemFlags(_serviceProvider,"Evil"),
                        Operator = Domain.AffectOperators.Or,
                    },
                    new Domain.CharacterSexAffectData
                    {
                        Value = Domain.Sex.Female,
                    },
                    new Domain.CharacterIRVAffectData
                    {
                        Location = Domain.IRVAffectLocations.Resistances,
                        Modifier = new IRVFlags(_serviceProvider,"Cold"),
                        Operator = Domain.AffectOperators.Add,
                    },
                    new Domain.CharacterFlagsAffectData
                    {
                        Modifier = new CharacterFlags(_serviceProvider,"Regeneration"),
                        Operator = Domain.AffectOperators.Nor,
                    },
                    new Domain.CharacterAttributeAffectData
                    {
                        Location = Domain.CharacterAttributeAffectLocations.SavingThrow,
                        Modifier = -5,
                        Operator = Domain.AffectOperators.Assign,
                    },
                ]
            };

            var internalAuraData = _serviceProvider.GetService<IMapper>()?.Map<Domain.AuraData, Filesystem.Domain.AuraData>(original);
            var externalAuraData = _serviceProvider.GetService<IMapper>()?.Map<Filesystem.Domain.AuraData, Domain.AuraData>(internalAuraData);

            original.WithDeepEqual(externalAuraData).Assert();
        }
    }
}
