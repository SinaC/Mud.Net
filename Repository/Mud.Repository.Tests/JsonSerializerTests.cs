using DeepEqual.Syntax;
using Mud.Domain;
using Mud.Domain.SerializationData.Account;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain.SerializationData;
using System.Text.Json;

namespace Mud.Repository.Tests
{
    [TestClass]
    public class JsonSerializerTests : MappingTestsBase
    {
        [TestMethod]
        public void SerializeThenDeserialize_Avatar()
        {
            var avatar = new AvatarData
            {
                Version = 1,
                AccountName = "account",
                CreationTime = DateTime.Now,
                RoomId = 0,
                SilverCoins = 0,
                GoldCoins = 0,
                Wimpy = 0,
                Experience = 0,
                Alignment = 0,
                Trains = 0,
                Practices = 0,
                AutoFlags = "Sacrifice,Affect",
                ActiveQuests = [],
                LearnedAbilities = [],
                LearnedAbilityGroups = [],
                Conditions = [],
                Aliases = [],
                Cooldowns = [],
                Pets = [],
                // CharacterBaseData
                Name = "Player",
                Race = "Human",
                Classes = ["Warrior"],
                Level = 1,
                Sex = Sex.Male,
                Size = Sizes.Medium,
                CurrentResources = new Dictionary<ResourceKinds, int>
                {
                    { ResourceKinds.HitPoints, 1 },
                    { ResourceKinds.MovePoints, 1 },
                },
                MaxResources = new Dictionary<ResourceKinds, int>
                {
                    { ResourceKinds.HitPoints, 1 },
                    { ResourceKinds.MovePoints, 1 },
                },
                Currencies = new Dictionary<Currencies, int>
                {
                    { Currencies.QuestPoints, 1 }
                },
                Equipments = [],
                Inventory = [],
                Auras = [],
                CharacterFlags = "Blind,DetectMagic",
                Immunities = "Fire,Pouet",
                Resistances = "Weapon,Wood",
                Vulnerabilities = string.Empty,
                ShieldFlags = string.Empty,
                Attributes = [],
            };

            var serialized = JsonSerializer.Serialize(avatar, _options);
            var deserialized = JsonSerializer.Deserialize<AvatarData>(serialized, _options)!;

            avatar.WithDeepEqual(deserialized).Assert();
        }

        [TestMethod]
        public void Deserialize_Avatar()
        {
            var avatarFileContent = File.ReadAllText(@"JsonFiles\avatar.json");
            var deserialized = JsonSerializer.Deserialize<AvatarData>(avatarFileContent, _options)!;

            Assert.IsNotNull(deserialized);
            Assert.AreEqual("fnar", deserialized.Name);
            Assert.IsEmpty(deserialized.Aliases);
            Assert.HasCount(7, deserialized.CurrentResources);
            Assert.HasCount(7, deserialized.MaxResources);
            Assert.ContainsSingle(deserialized.Pets);
            Assert.HasCount(15, deserialized.Equipments);
            Assert.ContainsSingle(deserialized.Equipments.Where(x => x.Item is ItemLightData));
            Assert.ContainsSingle(deserialized.Equipments.Where(x => x.Item is ItemWeaponData));
            Assert.HasCount(13, deserialized.Inventory);
            Assert.HasCount(13, deserialized.Auras);
            Assert.HasCount(12, deserialized.Attributes);
        }

        [TestMethod]
        public void Deserialize_Account()
        {
            var accountFileContent = File.ReadAllText(@"JsonFiles\account.json");
            var deserialized = JsonSerializer.Deserialize<AccountData>(accountFileContent, _options)!;

            Assert.IsNotNull(deserialized);
            Assert.AreEqual("sinac", deserialized.Username);
            Assert.HasCount(2, deserialized.Aliases);
            Assert.IsNotNull(deserialized.AdminData);
            Assert.AreEqual(AdminLevels.Implementor, deserialized.AdminData.AdminLevel);
            Assert.HasCount(3, deserialized.AvatarMetaDatas);
        }
    }
}
