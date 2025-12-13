using DeepEqual.Syntax;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Item;
using Mud.Server.Item;
using System.Text.Json;

namespace Mud.Repository.Tests
{
    [TestClass]
    public class JsonSerializerTests : MappingTestsBase
    {
        [TestMethod]
        public void SerializeThenDeserialize_SimplePC()
        {
            var pcData = new PlayableCharacterData
            {
                // CharacterBaseData
                Name = "Player",
                Race = "Human",
                Class = "Warrior",
                Level = 1,
                Sex = Sex.Male,
                Size = Sizes.Medium,
                HitPoints = 1,
                MovePoints = 1,
                CurrentResources = [],
                MaxResources = [],
                Equipments = [],
                Inventory = [],
                Auras = [],
                CharacterFlags = "Blind,DetectMagic",
                Immunities = "Fire,Pouet",
                Resistances = "Weapon,Wood",
                Vulnerabilities = string.Empty,
                ShieldFlags = string.Empty,
                Attributes = [],
                // PlayableCharacterData
                CreationTime = DateTime.Now,
                RoomId = 0,
                SilverCoins = 0,
                GoldCoins = 0,
                Experience = 0,
                Alignment = 0,
                Trains = 0,
                Practices = 0,
                AutoFlags = AutoFlags.None,
                CurrentQuests = [],
                LearnedAbilities = [],
                LearnedAbilityGroups = [],
                Conditions = [],
                Aliases = [],
                Cooldowns = [],
                Pets = []
            };

            var serialized = JsonSerializer.Serialize(pcData, _options);
            var deserialized = JsonSerializer.Deserialize<PlayableCharacterData>(serialized, _options)!;

            pcData.WithDeepEqual(deserialized).Assert();
        }

        [TestMethod]
        public void Deserialize_ComplexAdmin()
        {
            var deserialized = JsonSerializer.Deserialize<AdminData>(Pfile, _options)!;

            Assert.IsNotNull(deserialized);
            Assert.AreEqual("sinac", deserialized.Name);
            Assert.HasCount(2, deserialized.Aliases);
            Assert.HasCount(9, deserialized.Characters);
            Assert.HasCount(11, deserialized.Characters.Single(x => x.Name == "sinac").Equipments);
            Assert.ContainsSingle(deserialized.Characters.Single(x => x.Name == "sinac").Equipments.Where(x => x.Item is ItemLightData));
            Assert.ContainsSingle(deserialized.Characters.Single(x => x.Name == "sinac").Equipments.Where(x => x.Item is ItemWeaponData));
            Assert.ContainsSingle(deserialized.Characters.Single(x => x.Name == "sinac").Equipments.Where(x => x.Item is ItemWandData));
            Assert.HasCount(4, deserialized.Characters.Single(x => x.Name == "sinac").Inventory);
            Assert.HasCount(8, deserialized.Characters.Single(x => x.Name == "sinac").Auras);
            Assert.HasCount(14, deserialized.Characters.Single(x => x.Name == "sinac").Attributes);
        }

        private const string Pfile =
        @"{
  ""AdminLevel"": 7,
  ""WiznetFlags"": 61439,
  ""Name"": ""sinac"",
  ""PagingLineCount"": 0,
  ""Aliases"": {
    ""pouet"": ""who"",
    ""cda"": ""commanddebug admin""
  },
  ""Characters"": [
    {
      ""CreationTime"": ""2020-06-14T19:17:42.1765039+02:00"",
      ""RoomId"": 3014,
      ""SilverCoins"": 1049,
      ""GoldCoins"": 10,
      ""Experience"": 113000,
      ""Alignment"": -7,
      ""Trains"": 33,
      ""Practices"": 74,
      ""AutoFlags"": 127,
      ""CurrentQuests"": [
        {
          ""QuestId"": 1,
          ""StartTime"": ""2020-06-23T21:12:53.326539+02:00"",
          ""PulseLeft"": 0,
          ""CompletionTime"": null,
          ""GiverId"": 10,
          ""GiverRoomId"": 3025,
          ""Objectives"": [
            {
              ""ObjectiveId"": 1,
              ""Count"": 0
            },
            {
              ""ObjectiveId"": 2,
              ""Count"": 0
            },
            {
              ""ObjectiveId"": 0,
              ""Count"": 2
            },
            {
              ""ObjectiveId"": 3,
              ""Count"": 1
            }
          ]
        }
      ],
      ""LearnedAbilities"": [
        {
          ""Name"": ""Envenom"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 94,
          ""Rating"": 1
        },
        {
          ""Name"": ""Backstab"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Bash"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Berserk"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 94,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dirt Kicking"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Disarm"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Hide"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Kick"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Pick Lock"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Recall"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Rescue"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Scrolls"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Sneak"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Staves"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Trip"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Wands"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Axe"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dagger"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dodge"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 71,
          ""Rating"": 1
        },
        {
          ""Name"": ""Enhanced Damage"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 71,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fast Healing"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 71,
          ""Rating"": 1
        },
        {
          ""Name"": ""Flail"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Haggle"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Hand To Hand"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Mace"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Meditation"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Parry"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 71,
          ""Rating"": 1
        },
        {
          ""Name"": ""Peek"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Polearm"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Second Attack"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Shield Block"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Spear"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Staff(weapon)"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Sword"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 94,
          ""Rating"": 1
        },
        {
          ""Name"": ""Third Attack"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Whip"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Test"",
          ""ResourceKind"": null,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Construct"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Acid Blast"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Bless"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Blindness"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Burning Hands"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Call Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Calm"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cancellation"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cause Critical"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cause Light"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cause Serious"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chain Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Change Sex"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Charm Person"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chill Touch"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Colour Spray"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Continual Light"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Control Weather"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Food"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Rose"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Spring"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Water"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Blindness"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Critical"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Disease"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Light"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Poison"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Serious"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Curse"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Demonfire"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Evil"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Good"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Hidden"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Invis"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Poison"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dispel Evil"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dispel Good"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dispel Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Earthquake"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Enchant Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Enchant Weapon"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Energy Drain"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fire"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fog"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Farsight"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireball"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireproof"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Flamestrike"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Floating Disc"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fly"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Frenzy"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Gate"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Giant Strength"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Harm"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Haste"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Heal"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Heat Metal"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Holy Word"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Identify"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Infravision"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Invisibility"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Known Alignment"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Lightning Bolt"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Locate Object"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Magic Missile"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Mass Healing"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Mass Invis"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Nexus"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Pass Door"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Plague"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Poison"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Portal"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Protection Evil"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Protection Good"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Ray Of Truth"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Recharge"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Refresh"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Remove Curse"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Sanctuary"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Shield"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Shocking Grasp"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Sleep"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Slow"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Stone Skin"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Summon"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Teleport"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Ventriloquate"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Weaken"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Word Of Recall"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""General Purpose"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""High Explosive"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Acid Breath"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fire Breath"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Frost Breath"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Gas Breath"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Lightning Breath"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fourth Wield"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 0,
          ""Rating"": 0
        },
        {
          ""Name"": ""Dual Wield"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 0,
          ""Rating"": 0
        },
        {
          ""Name"": ""Third Wield"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 0,
          ""Rating"": 0
        },
        {
          ""Name"": ""Steal"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Bite"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Crush"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Tail"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Testroom"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Giant Size"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        }
      ],
      ""Conditions"": {
        ""Drunk"": 0,
        ""Full"": 24,
        ""Thirst"": 32,
        ""Hunger"": 38
      },
      ""Aliases"": {},
      ""Cooldowns"": {},
      ""Pets"": [],
      ""Name"": ""sinac"",
      ""Race"": ""insectoid"",
      ""Class"": ""druid"",
      ""Level"": 46,
      ""Sex"": 1,
      ""Size"": 2,
      ""HitPoints"": 13800,
      ""MovePoints"": 13786,
      ""CurrentResources"": {
        ""Mana"": 3151,
        ""Psy"": 100
      },
      ""MaxResources"": {
        ""Mana"": 10000,
        ""Psy"": 100
      },
      ""Equipments"": [
        {
          ""Slot"": 1,
          ""Item"": {
            ""$type"": ""light"",
            ""TimeLeft"": 29986,
            ""ItemId"": 3365,
            ""Level"": 8,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": ""Glowing,Magic"",
            ""Auras"": [
              {
                ""AbilityName"": ""Test"",
                ""Level"": 18,
                ""PulseLeft"": 1900,
                ""AuraFlags"": 2,
                ""Affects"": [
                  {
                    ""$type"": ""itemFlags"",
                    ""Operator"": 0,
                    ""Modifier"": ""Glowing,Humming,Magic""
                  },
                  {
                    ""$type"": ""characterAttribute"",
                    ""Operator"": 0,
                    ""Location"": 16,
                    ""Modifier"": -46
                  },
                  {
                    ""$type"": ""characterAttribute"",
                    ""Operator"": 0,
                    ""Location"": 6,
                    ""Modifier"": 46
                  }
                ]
              }
            ]
          }
        },
        {
          ""Slot"": 4,
          ""Item"": {
            ""$type"": ""base"",
            ""ItemId"": 3353,
            ""Level"": 5,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": """",
            ""Auras"": []
          }
        },
        {
          ""Slot"": 5,
          ""Item"": {
            ""$type"": ""base"",
            ""ItemId"": 3361,
            ""Level"": 3,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": """",
            ""Auras"": []
          }
        },
        {
          ""Slot"": 6,
          ""Item"": {
            ""$type"": ""base"",
            ""ItemId"": 3362,
            ""Level"": 5,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": """",
            ""Auras"": []
          }
        },
        {
          ""Slot"": 9,
          ""Item"": {
            ""$type"": ""base"",
            ""ItemId"": 3359,
            ""Level"": 5,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": """",
            ""Auras"": []
          }
        },
        {
          ""Slot"": 10,
          ""Item"": {
            ""$type"": ""base"",
            ""ItemId"": 3364,
            ""Level"": 16,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": ""Magic"",
            ""Auras"": []
          }
        },
        {
          ""Slot"": 10,
          ""Item"": {
            ""$type"": ""base"",
            ""ItemId"": 3364,
            ""Level"": 16,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": ""Magic"",
            ""Auras"": []
          }
        },
        {
          ""Slot"": 10,
          ""Item"": {
            ""$type"": ""base"",
            ""ItemId"": 3364,
            ""Level"": 16,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": ""Magic"",
            ""Auras"": []
          }
        },
        {
          ""Slot"": 10,
          ""Item"": {
            ""$type"": ""base"",
            ""ItemId"": 3364,
            ""Level"": 16,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": ""Magic"",
            ""Auras"": []
          }
        },
        {
          ""Slot"": 13,
          ""Item"": {
            ""$type"": ""weapon"",
            ""WeaponFlags"": ""TwoHands,Vorpal"",
            ""ItemId"": 3005,
            ""Level"": 42,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": ""Bless,Humming"",
            ""Auras"": [
              {
                ""AbilityName"": ""Test"",
                ""Level"": 1,
                ""PulseLeft"": 3376,
                ""AuraFlags"": 2,
                ""Affects"": [
                  {
                    ""$type"": ""weaponFlags"",
                    ""Operator"": 0,
                    ""Modifier"": ""Flaming,Frost,Holy,Poison,Sharp,Shocking,Vampiric""
                  }
                ]
              }
            ]
          }
        },
        {
          ""Slot"": 14,
          ""Item"": {
            ""$type"": ""wand"",
            ""MaxChargeCount"": 1,
            ""CurrentChargeCount"": 1,
            ""AlreadyRecharged"": false,
            ""ItemId"": 106,
            ""Level"": 3,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": """",
            ""Auras"": []
          }
        }
      ],
      ""Inventory"": [
        {
          ""$type"": ""food"",
          ""FullHours"": 8,
          ""HungerHours"": 6,
          ""IsPoisoned"": false,
          ""ItemId"": 3014,
          ""Level"": 0,
          ""DecayPulseLeft"": 0,
          ""ItemFlags"": """",
          ""Auras"": []
        },
        {
          ""$type"": ""food"",
          ""FullHours"": 8,
          ""HungerHours"": 6,
          ""IsPoisoned"": false,
          ""ItemId"": 3014,
          ""Level"": 0,
          ""DecayPulseLeft"": 0,
          ""ItemFlags"": """",
          ""Auras"": []
        },
        {
          ""$type"": ""food"",
          ""FullHours"": 8,
          ""HungerHours"": 6,
          ""IsPoisoned"": false,
          ""ItemId"": 3014,
          ""Level"": 0,
          ""DecayPulseLeft"": 0,
          ""ItemFlags"": """",
          ""Auras"": []
        },
        {
          ""$type"": ""weapon"",
          ""WeaponFlags"": """",
          ""ItemId"": 3350,
          ""Level"": 16,
          ""DecayPulseLeft"": 0,
          ""ItemFlags"": ""Magic"",
          ""Auras"": []
        }
      ],
      ""Auras"": [
        {
          ""AbilityName"": ""Sanctuary"",
          ""Level"": 13,
          ""PulseLeft"": 960,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""shieldFlags"",
              ""Operator"": 1,
              ""Modifier"": ""Sanctuary""
            }
          ]
        },
        {
          ""AbilityName"": ""Shield"",
          ""Level"": 20,
          ""PulseLeft"": 12252,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""characterAttribute"",
              ""Operator"": 0,
              ""Location"": 16,
              ""Modifier"": -20
            }
          ]
        },
        {
          ""AbilityName"": ""Berserk"",
          ""Level"": 7,
          ""PulseLeft"": 272,
          ""AuraFlags"": 2,
          ""Affects"": [
            {
              ""$type"": ""characterFlags"",
              ""Operator"": 1,
              ""Modifier"": ""Berserk""
            },
            {
              ""$type"": ""characterAttribute"",
              ""Operator"": 0,
              ""Location"": 9,
              ""Modifier"": 9
            },
            {
              ""$type"": ""characterAttribute"",
              ""Operator"": 0,
              ""Location"": 10,
              ""Modifier"": 9
            },
            {
              ""$type"": ""characterAttribute"",
              ""Operator"": 0,
              ""Location"": 16,
              ""Modifier"": 90
            }
          ]
        },
        {
          ""AbilityName"": ""Stone Skin"",
          ""Level"": 8,
          ""PulseLeft"": 10372,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""characterAttribute"",
              ""Operator"": 0,
              ""Location"": 16,
              ""Modifier"": -40
            }
          ]
        },
        {
          ""AbilityName"": ""Protection Evil"",
          ""Level"": 9,
          ""PulseLeft"": 5108,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""characterAttribute"",
              ""Operator"": 0,
              ""Location"": 8,
              ""Modifier"": -1
            },
            {
              ""$type"": ""shieldFlags"",
              ""Operator"": 1,
              ""Modifier"": ""ProtectEvil""
            }
          ]
        },
        {
          ""AbilityName"": ""Testroom"",
          ""Level"": 23,
          ""PulseLeft"": 868,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""roomHealRate"",
              ""Operator"": 0,
              ""Modifier"": 10
            },
            {
              ""$type"": ""roomSourceRate"",
              ""Operator"": 2,
              ""Modifier"": 150
            },
            {
              ""$type"": ""roomFlags"",
              ""Operator"": 1,
              ""Modifier"": ""NoScan,NoWhere""
            }
          ]
        },
        {
          ""AbilityName"": ""Plague"",
          ""Level"": 17,
          ""PulseLeft"": 10752,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""characterAttribute"",
              ""Operator"": 0,
              ""Location"": 1,
              ""Modifier"": -5
            },
            {
              ""$type"": ""characterFlags"",
              ""Operator"": 1,
              ""Modifier"": ""Plague""
            },
            {
              ""$type"": ""plague""
            }
          ]
        },
        {
          ""AbilityName"": ""Poison"",
          ""Level"": 31,
          ""PulseLeft"": 10848,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""characterAttribute"",
              ""Operator"": 0,
              ""Location"": 1,
              ""Modifier"": -2
            },
            {
              ""$type"": ""characterFlags"",
              ""Operator"": 1,
              ""Modifier"": ""Poison""
            },
            {
              ""$type"": ""poison""
            }
          ]
        }
      ],
      ""CharacterFlags"": ""Berserk"",
      ""Immunities"": """",
      ""Resistances"": ""Acid,Bash,Disease,Poison,Slash"",
      ""Vulnerabilities"": ""Cold,Fire,Pierce"",
      ""ShieldFlags"": ""ProtectEvil,Sanctuary"",
      ""Attributes"": {
        ""Strength"": 25,
        ""Intelligence"": 24,
        ""Wisdom"": 23,
        ""Dexterity"": 22,
        ""Constitution"": 27,
        ""MaxHitPoints"": 13800,
        ""SavingThrow"": 0,
        ""HitRoll"": 123,
        ""DamRoll"": 123,
        ""MaxMovePoints"": 13800,
        ""ArmorBash"": -500,
        ""ArmorPierce"": -500,
        ""ArmorSlash"": -500,
        ""ArmorExotic"": -500
      }
    },
    {
      ""CreationTime"": ""2020-06-23T15:52:16.2575062+02:00"",
      ""RoomId"": 3001,
      ""SilverCoins"": 0,
      ""GoldCoins"": 0,
      ""Experience"": 0,
      ""Alignment"": 0,
      ""Trains"": 3,
      ""Practices"": 5,
      ""AutoFlags"": 0,
      ""CurrentQuests"": [],
      ""LearnedAbilities"": [
        {
          ""Name"": ""Berserk"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        }
      ],
      ""Conditions"": {
        ""Drunk"": 0,
        ""Full"": 48,
        ""Thirst"": 48,
        ""Hunger"": 48
      },
      ""Aliases"": {},
      ""Cooldowns"": {},
      ""Pets"": [],
      ""Name"": ""pouet"",
      ""Race"": ""dwarf"",
      ""Class"": ""cleric"",
      ""Level"": 1,
      ""Sex"": 1,
      ""Size"": 1,
      ""HitPoints"": 100,
      ""MovePoints"": 100,
      ""CurrentResources"": {
        ""Mana"": 100,
        ""Psy"": 100
      },
      ""MaxResources"": {
        ""Mana"": 100,
        ""Psy"": 100
      },
      ""Equipments"": [],
      ""Inventory"": [],
      ""Auras"": [],
      ""CharacterFlags"": """",
      ""Immunities"": """",
      ""Resistances"": ""Disease,Poison"",
      ""Vulnerabilities"": ""Drowning"",
      ""ShieldFlags"": """",
      ""Attributes"": {
        ""Strength"": 14,
        ""Intelligence"": 12,
        ""Wisdom"": 16,
        ""Dexterity"": 10,
        ""Constitution"": 15,
        ""MaxHitPoints"": 100,
        ""SavingThrow"": 0,
        ""HitRoll"": 0,
        ""DamRoll"": 0,
        ""MaxMovePoints"": 100,
        ""ArmorBash"": 100,
        ""ArmorPierce"": 100,
        ""ArmorSlash"": 100,
        ""ArmorExotic"": 100
      }
    },
    {
      ""CreationTime"": ""2020-07-03T08:51:55.1055791+02:00"",
      ""RoomId"": 7018,
      ""SilverCoins"": 2,
      ""GoldCoins"": 0,
      ""Experience"": 1488,
      ""Alignment"": 0,
      ""Trains"": 4,
      ""Practices"": 6,
      ""AutoFlags"": 15,
      ""CurrentQuests"": [],
      ""LearnedAbilities"": [
        {
          ""Name"": ""Detect Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 2,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Invis"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 3,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chill Touch"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 4,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Continual Light"",
          ""ResourceKind"": 0,
          ""CostAmount"": 7,
          ""CostAmountOperator"": 1,
          ""Level"": 6,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fire"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 6,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 7,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Burning Hands"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 7,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Water"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 8,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Food"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fly"",
          ""ResourceKind"": 0,
          ""CostAmount"": 10,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Floating Disc"",
          ""ResourceKind"": 0,
          ""CostAmount"": 4,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Evil"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 11,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Good"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 11,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Blindness"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 12,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireproof"",
          ""ResourceKind"": 0,
          ""CostAmount"": 10,
          ""CostAmountOperator"": 1,
          ""Level"": 13,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Spring"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fog"",
          ""ResourceKind"": 0,
          ""CostAmount"": 12,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Farsight"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Control Weather"",
          ""ResourceKind"": 0,
          ""CostAmount"": 25,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Hidden"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Poison"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Colour Spray"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Rose"",
          ""ResourceKind"": 0,
          ""CostAmount"": 30,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dispel Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Enchant Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 100,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 2
        },
        {
          ""Name"": ""Enchant Weapon"",
          ""ResourceKind"": 0,
          ""CostAmount"": 100,
          ""CostAmountOperator"": 1,
          ""Level"": 17,
          ""Learned"": 0,
          ""Rating"": 2
        },
        {
          ""Name"": ""Cancellation"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 18,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Curse"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 18,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Energy Drain"",
          ""ResourceKind"": 0,
          ""CostAmount"": 35,
          ""CostAmountOperator"": 1,
          ""Level"": 19,
          ""Learned"": 0,
          ""Rating"": 2
        },
        {
          ""Name"": ""Charm Person"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireball"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 22,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Call Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 26,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Acid Blast"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 28,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chain Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 25,
          ""CostAmountOperator"": 1,
          ""Level"": 33,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Calm"",
          ""ResourceKind"": 0,
          ""CostAmount"": 30,
          ""CostAmountOperator"": 1,
          ""Level"": 48,
          ""Learned"": 0,
          ""Rating"": 1
        }
      ],
      ""Conditions"": {
        ""Drunk"": 0,
        ""Full"": 6,
        ""Thirst"": 27,
        ""Hunger"": 27
      },
      ""Aliases"": {},
      ""Cooldowns"": {},
      ""Pets"": [],
      ""Name"": ""xiinobis"",
      ""Race"": ""human"",
      ""Class"": ""mage"",
      ""Level"": 2,
      ""Sex"": 1,
      ""Size"": 2,
      ""HitPoints"": 84,
      ""MovePoints"": 106,
      ""CurrentResources"": {
        ""Mana"": 18,
        ""Psy"": 100
      },
      ""MaxResources"": {
        ""Mana"": 18,
        ""Psy"": 100
      },
      ""Equipments"": [],
      ""Inventory"": [],
      ""Auras"": [],
      ""CharacterFlags"": """",
      ""Immunities"": """",
      ""Resistances"": """",
      ""Vulnerabilities"": """",
      ""ShieldFlags"": """",
      ""Attributes"": {
        ""Strength"": 13,
        ""Intelligence"": 15,
        ""Wisdom"": 13,
        ""Dexterity"": 13,
        ""Constitution"": 13,
        ""MaxHitPoints"": 106,
        ""SavingThrow"": 0,
        ""HitRoll"": 0,
        ""DamRoll"": 0,
        ""MaxMovePoints"": 106,
        ""ArmorBash"": 100,
        ""ArmorPierce"": 100,
        ""ArmorSlash"": 100,
        ""ArmorExotic"": 100
      }
    },
    {
      ""CreationTime"": ""2025-11-08T11:24:32.7520387+01:00"",
      ""RoomId"": 3001,
      ""SilverCoins"": 0,
      ""GoldCoins"": 0,
      ""Experience"": 0,
      ""Alignment"": 0,
      ""Trains"": 3,
      ""Practices"": 5,
      ""AutoFlags"": 0,
      ""CurrentQuests"": [],
      ""LearnedAbilities"": [
        {
          ""Name"": ""Bash"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fast healing"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        }
      ],
      ""Conditions"": {
        ""Drunk"": 0,
        ""Full"": 44,
        ""Thirst"": 47,
        ""Hunger"": 46
      },
      ""Aliases"": {},
      ""Cooldowns"": {},
      ""Pets"": [],
      ""Name"": ""fnar"",
      ""Race"": ""giant"",
      ""Class"": ""warrior"",
      ""Level"": 1,
      ""Sex"": 1,
      ""Size"": 3,
      ""HitPoints"": 100,
      ""MovePoints"": 100,
      ""CurrentResources"": {
        ""Mana"": 100,
        ""Psy"": 100
      },
      ""MaxResources"": {
        ""Mana"": 100,
        ""Psy"": 100
      },
      ""Equipments"": [],
      ""Inventory"": [],
      ""Auras"": [],
      ""CharacterFlags"": """",
      ""Immunities"": """",
      ""Resistances"": ""Cold,Fire"",
      ""Vulnerabilities"": ""Lightning,Mental"",
      ""ShieldFlags"": """",
      ""Attributes"": {
        ""Strength"": 18,
        ""Intelligence"": 11,
        ""Wisdom"": 13,
        ""Dexterity"": 11,
        ""Constitution"": 14,
        ""MaxHitPoints"": 100,
        ""SavingThrow"": 0,
        ""HitRoll"": 0,
        ""DamRoll"": 0,
        ""MaxMovePoints"": 100,
        ""ArmorBash"": 100,
        ""ArmorPierce"": 100,
        ""ArmorSlash"": 100,
        ""ArmorExotic"": 100
      }
    },
    {
      ""CreationTime"": ""2025-11-08T11:29:28.5244257+01:00"",
      ""RoomId"": 3001,
      ""SilverCoins"": 0,
      ""GoldCoins"": 0,
      ""Experience"": 0,
      ""Alignment"": 0,
      ""Trains"": 3,
      ""Practices"": 5,
      ""AutoFlags"": 0,
      ""CurrentQuests"": [],
      ""LearnedAbilities"": [
        {
          ""Name"": ""Detect Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 2,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Invis"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 3,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chill Touch"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 4,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Continual Light"",
          ""ResourceKind"": 0,
          ""CostAmount"": 7,
          ""CostAmountOperator"": 1,
          ""Level"": 6,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fire"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 6,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 7,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Burning Hands"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 7,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Water"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 8,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Food"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fly"",
          ""ResourceKind"": 0,
          ""CostAmount"": 10,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Floating Disc"",
          ""ResourceKind"": 0,
          ""CostAmount"": 4,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Evil"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 11,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Good"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 11,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Blindness"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 12,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireproof"",
          ""ResourceKind"": 0,
          ""CostAmount"": 10,
          ""CostAmountOperator"": 1,
          ""Level"": 13,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Spring"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fog"",
          ""ResourceKind"": 0,
          ""CostAmount"": 12,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Farsight"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Control Weather"",
          ""ResourceKind"": 0,
          ""CostAmount"": 25,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Hidden"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Poison"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Colour Spray"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Rose"",
          ""ResourceKind"": 0,
          ""CostAmount"": 30,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dispel Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Enchant Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 100,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 2
        },
        {
          ""Name"": ""Enchant Weapon"",
          ""ResourceKind"": 0,
          ""CostAmount"": 100,
          ""CostAmountOperator"": 1,
          ""Level"": 17,
          ""Learned"": 0,
          ""Rating"": 2
        },
        {
          ""Name"": ""Cancellation"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 18,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Curse"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 18,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Energy Drain"",
          ""ResourceKind"": 0,
          ""CostAmount"": 35,
          ""CostAmountOperator"": 1,
          ""Level"": 19,
          ""Learned"": 0,
          ""Rating"": 2
        },
        {
          ""Name"": ""Charm Person"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireball"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 22,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Call Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 26,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Acid Blast"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 28,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chain Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 25,
          ""CostAmountOperator"": 1,
          ""Level"": 33,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Calm"",
          ""ResourceKind"": 0,
          ""CostAmount"": 30,
          ""CostAmountOperator"": 1,
          ""Level"": 48,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Sneak"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Hide"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        }
      ],
      ""Conditions"": {
        ""Drunk"": 0,
        ""Full"": 44,
        ""Thirst"": 46,
        ""Hunger"": 46
      },
      ""Aliases"": {},
      ""Cooldowns"": {},
      ""Pets"": [],
      ""Name"": ""pouetpouet"",
      ""Race"": ""elf"",
      ""Class"": ""mage"",
      ""Level"": 1,
      ""Sex"": 0,
      ""Size"": 2,
      ""HitPoints"": 100,
      ""MovePoints"": 100,
      ""CurrentResources"": {
        ""Mana"": 100,
        ""Psy"": 100
      },
      ""MaxResources"": {
        ""Mana"": 100,
        ""Psy"": 100
      },
      ""Equipments"": [],
      ""Inventory"": [],
      ""Auras"": [],
      ""CharacterFlags"": """",
      ""Immunities"": """",
      ""Resistances"": ""Charm"",
      ""Vulnerabilities"": ""Iron"",
      ""ShieldFlags"": """",
      ""Attributes"": {
        ""Strength"": 12,
        ""Intelligence"": 16,
        ""Wisdom"": 13,
        ""Dexterity"": 15,
        ""Constitution"": 11,
        ""MaxHitPoints"": 100,
        ""SavingThrow"": 0,
        ""HitRoll"": 0,
        ""DamRoll"": 0,
        ""MaxMovePoints"": 100,
        ""ArmorBash"": 100,
        ""ArmorPierce"": 100,
        ""ArmorSlash"": 100,
        ""ArmorExotic"": 100
      }
    },
    {
      ""CreationTime"": ""2025-11-08T11:32:19.9330366+01:00"",
      ""RoomId"": 3019,
      ""SilverCoins"": 0,
      ""GoldCoins"": 0,
      ""Experience"": 1000,
      ""Alignment"": 0,
      ""Trains"": 4,
      ""Practices"": 6,
      ""AutoFlags"": 0,
      ""CurrentQuests"": [],
      ""LearnedAbilities"": [
        {
          ""Name"": ""Detect Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 2,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Invis"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 3,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chill Touch"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 4,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Continual Light"",
          ""ResourceKind"": 0,
          ""CostAmount"": 7,
          ""CostAmountOperator"": 1,
          ""Level"": 6,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fire"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 6,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 7,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Burning Hands"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 7,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Water"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 8,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Food"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fly"",
          ""ResourceKind"": 0,
          ""CostAmount"": 10,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Floating Disc"",
          ""ResourceKind"": 0,
          ""CostAmount"": 4,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Evil"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 11,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Good"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 11,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Blindness"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 12,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireproof"",
          ""ResourceKind"": 0,
          ""CostAmount"": 10,
          ""CostAmountOperator"": 1,
          ""Level"": 13,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Spring"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fog"",
          ""ResourceKind"": 0,
          ""CostAmount"": 12,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Farsight"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Control Weather"",
          ""ResourceKind"": 0,
          ""CostAmount"": 25,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Hidden"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Poison"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Colour Spray"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Rose"",
          ""ResourceKind"": 0,
          ""CostAmount"": 30,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dispel Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Enchant Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 100,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 0,
          ""Rating"": 2
        },
        {
          ""Name"": ""Enchant Weapon"",
          ""ResourceKind"": 0,
          ""CostAmount"": 100,
          ""CostAmountOperator"": 1,
          ""Level"": 17,
          ""Learned"": 0,
          ""Rating"": 2
        },
        {
          ""Name"": ""Cancellation"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 18,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Curse"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 18,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Energy Drain"",
          ""ResourceKind"": 0,
          ""CostAmount"": 35,
          ""CostAmountOperator"": 1,
          ""Level"": 19,
          ""Learned"": 0,
          ""Rating"": 2
        },
        {
          ""Name"": ""Charm Person"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 20,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireball"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 22,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Call Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 26,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Acid Blast"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 28,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chain Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 25,
          ""CostAmountOperator"": 1,
          ""Level"": 33,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Calm"",
          ""ResourceKind"": 0,
          ""CostAmount"": 30,
          ""CostAmountOperator"": 1,
          ""Level"": 48,
          ""Learned"": 0,
          ""Rating"": 1
        },
        {
          ""Name"": ""Sneak"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Hide"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        }
      ],
      ""Conditions"": {
        ""Drunk"": 0,
        ""Full"": 46,
        ""Thirst"": 47,
        ""Hunger"": 47
      },
      ""Aliases"": {},
      ""Cooldowns"": {},
      ""Pets"": [],
      ""Name"": ""pp"",
      ""Race"": ""elf"",
      ""Class"": ""mage"",
      ""Level"": 2,
      ""Sex"": 0,
      ""Size"": 2,
      ""HitPoints"": 105,
      ""MovePoints"": 83,
      ""CurrentResources"": {
        ""Mana"": 122,
        ""Psy"": 100
      },
      ""MaxResources"": {
        ""Mana"": 122,
        ""Psy"": 100
      },
      ""Equipments"": [],
      ""Inventory"": [],
      ""Auras"": [],
      ""CharacterFlags"": """",
      ""Immunities"": """",
      ""Resistances"": ""Charm"",
      ""Vulnerabilities"": ""Iron"",
      ""ShieldFlags"": """",
      ""Attributes"": {
        ""Strength"": 12,
        ""Intelligence"": 16,
        ""Wisdom"": 13,
        ""Dexterity"": 15,
        ""Constitution"": 11,
        ""MaxHitPoints"": 105,
        ""SavingThrow"": 0,
        ""HitRoll"": 0,
        ""DamRoll"": 0,
        ""MaxMovePoints"": 105,
        ""ArmorBash"": 100,
        ""ArmorPierce"": 100,
        ""ArmorSlash"": 100,
        ""ArmorExotic"": 100
      }
    },
    {
      ""CreationTime"": ""2025-11-08T11:38:19.6902431+01:00"",
      ""RoomId"": 3019,
      ""SilverCoins"": 0,
      ""GoldCoins"": 0,
      ""Experience"": 1000,
      ""Alignment"": 0,
      ""Trains"": 4,
      ""Practices"": 4,
      ""AutoFlags"": 0,
      ""CurrentQuests"": [],
      ""LearnedAbilities"": [
        {
          ""Name"": ""Detect Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 2,
          ""Learned"": 63,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Invis"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 3,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chill Touch"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 4,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Continual Light"",
          ""ResourceKind"": 0,
          ""CostAmount"": 7,
          ""CostAmountOperator"": 1,
          ""Level"": 6,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fire"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 6,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 7,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Burning Hands"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 7,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Water"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 8,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Food"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fly"",
          ""ResourceKind"": 0,
          ""CostAmount"": 10,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Floating Disc"",
          ""ResourceKind"": 0,
          ""CostAmount"": 4,
          ""CostAmountOperator"": 1,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Evil"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 11,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Good"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 11,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Blindness"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 12,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireproof"",
          ""ResourceKind"": 0,
          ""CostAmount"": 10,
          ""CostAmountOperator"": 1,
          ""Level"": 13,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Spring"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fog"",
          ""ResourceKind"": 0,
          ""CostAmount"": 12,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Farsight"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 14,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Control Weather"",
          ""ResourceKind"": 0,
          ""CostAmount"": 25,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Hidden"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Poison"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 15,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Colour Spray"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Rose"",
          ""ResourceKind"": 0,
          ""CostAmount"": 30,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dispel Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Enchant Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 100,
          ""CostAmountOperator"": 1,
          ""Level"": 16,
          ""Learned"": 1,
          ""Rating"": 2
        },
        {
          ""Name"": ""Enchant Weapon"",
          ""ResourceKind"": 0,
          ""CostAmount"": 100,
          ""CostAmountOperator"": 1,
          ""Level"": 17,
          ""Learned"": 1,
          ""Rating"": 2
        },
        {
          ""Name"": ""Cancellation"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 18,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Curse"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 18,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Energy Drain"",
          ""ResourceKind"": 0,
          ""CostAmount"": 35,
          ""CostAmountOperator"": 1,
          ""Level"": 19,
          ""Learned"": 1,
          ""Rating"": 2
        },
        {
          ""Name"": ""Charm Person"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 1,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireball"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 22,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Call Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 15,
          ""CostAmountOperator"": 1,
          ""Level"": 26,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Acid Blast"",
          ""ResourceKind"": 0,
          ""CostAmount"": 20,
          ""CostAmountOperator"": 1,
          ""Level"": 28,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chain Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 25,
          ""CostAmountOperator"": 1,
          ""Level"": 33,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Calm"",
          ""ResourceKind"": 0,
          ""CostAmount"": 30,
          ""CostAmountOperator"": 1,
          ""Level"": 48,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Sneak"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Hide"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        }
      ],
      ""Conditions"": {
        ""Drunk"": 0,
        ""Full"": 44,
        ""Thirst"": 46,
        ""Hunger"": 46
      },
      ""Aliases"": {},
      ""Cooldowns"": {},
      ""Pets"": [],
      ""Name"": ""ppp"",
      ""Race"": ""elf"",
      ""Class"": ""mage"",
      ""Level"": 2,
      ""Sex"": 0,
      ""Size"": 2,
      ""HitPoints"": 105,
      ""MovePoints"": 105,
      ""CurrentResources"": {
        ""Mana"": 105,
        ""Psy"": 100
      },
      ""MaxResources"": {
        ""Mana"": 105,
        ""Psy"": 100
      },
      ""Equipments"": [],
      ""Inventory"": [],
      ""Auras"": [],
      ""CharacterFlags"": """",
      ""Immunities"": """",
      ""Resistances"": ""Charm"",
      ""Vulnerabilities"": ""Iron"",
      ""ShieldFlags"": """",
      ""Attributes"": {
        ""Strength"": 12,
        ""Intelligence"": 16,
        ""Wisdom"": 13,
        ""Dexterity"": 15,
        ""Constitution"": 11,
        ""MaxHitPoints"": 105,
        ""SavingThrow"": 0,
        ""HitRoll"": 0,
        ""DamRoll"": 0,
        ""MaxMovePoints"": 105,
        ""ArmorBash"": 100,
        ""ArmorPierce"": 100,
        ""ArmorSlash"": 100,
        ""ArmorExotic"": 100
      }
    },
    {
      ""CreationTime"": ""2025-11-11T16:31:33.7841739+01:00"",
      ""RoomId"": 1309,
      ""SilverCoins"": 0,
      ""GoldCoins"": 0,
      ""Experience"": 120002,
      ""Alignment"": 0,
      ""Trains"": 22,
      ""Practices"": 91,
      ""AutoFlags"": 127,
      ""CurrentQuests"": [],
      ""LearnedAbilities"": [
        {
          ""Name"": ""Backstab"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Bash"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Berserk"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dirt Kicking"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Disarm"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Envenom"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Hide"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Kick"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Pick Lock"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Recall"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Rescue"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Scrolls"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Sneak"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Staves"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Steal"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Trip"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Wands"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Bite"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Crush"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Tail"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fourth Wield"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 1,
          ""Rating"": 0
        },
        {
          ""Name"": ""Dual Wield"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 1,
          ""Rating"": 0
        },
        {
          ""Name"": ""Third Wield"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 1,
          ""Rating"": 0
        },
        {
          ""Name"": ""Axe"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dagger"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dodge"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Enhanced Damage"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fast Healing"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Flail"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Haggle"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Hand to Hand"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Mace"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Meditation"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Parry"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 2,
          ""Rating"": 1
        },
        {
          ""Name"": ""Peek"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Polearm"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Second Attack"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Shield Block"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Spear"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Staff(weapon)"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Sword"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Third Attack"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Whip"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 10,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Test"",
          ""ResourceKind"": null,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 0,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Construct"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Testroom"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Giant Size"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Acid Blast"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Bless"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Blindness"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Burning Hands"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Call Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Calm"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cancellation"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cause Critical"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cause Light"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cause Serious"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chain Lightning"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Change Sex"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Charm Person"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Chill Touch"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Colour Spray"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Continual Light"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Control Weather"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Food"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Rose"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Spring"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Create Water"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Blindness"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Critical"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Disease"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Light"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Poison"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cure Serious"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Curse"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Demonfire"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Evil"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Good"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Hidden"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Invis"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Detect Poison"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dispel Evil"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dispel Good"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Dispel Magic"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Earthquake"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Enchant Armor"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Enchant Weapon"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Energy Drain"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fire"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Faerie Fog"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Farsight"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireball"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fireproof"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Flamestrike"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Floating Disc"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fly"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Frenzy"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Gate"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Giant Strength"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Harm"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Haste"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Heal"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Heat Metal"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Holy Word"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Identify"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Infravision"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Invisibility"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Known Alignment"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Lightning Bolt"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Locate Object"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Magic Missile"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Mass Healing"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Mass Invis"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Nexus"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Pass Door"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Plague"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Poison"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Portal"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Protection Evil"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Protection Good"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Ray of Truth"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Recharge"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Refresh"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Remove Curse"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Sanctuary"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Shield"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Shocking Grasp"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Sleep"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Slow"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Stone Skin"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Summon"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Teleport"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Ventriloquate"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Weaken"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Word of Recall"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""General Purpose"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""High Explosive"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Acid Breath"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fire Breath"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Frost Breath"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Gas Breath"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Lightning Breath"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Cat Form"",
          ""ResourceKind"": null,
          ""CostAmount"": 0,
          ""CostAmountOperator"": 0,
          ""Level"": 20,
          ""Learned"": 1,
          ""Rating"": 1
        },
        {
          ""Name"": ""Fire Shield"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Ice Shield"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Shadow Word: Pain"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        },
        {
          ""Name"": ""Shock Shield"",
          ""ResourceKind"": 0,
          ""CostAmount"": 5,
          ""CostAmountOperator"": 2,
          ""Level"": 1,
          ""Learned"": 100,
          ""Rating"": 1
        }
      ],
      ""Conditions"": {
        ""Drunk"": 0,
        ""Full"": 8,
        ""Thirst"": 28,
        ""Hunger"": 28
      },
      ""Aliases"": {},
      ""Cooldowns"": {},
      ""Pets"": [],
      ""Name"": ""glouk"",
      ""Race"": ""insectoid"",
      ""Class"": ""druid"",
      ""Level"": 49,
      ""Sex"": 0,
      ""Size"": 2,
      ""HitPoints"": 218,
      ""MovePoints"": 200,
      ""CurrentResources"": {
        ""Mana"": 186,
        ""Psy"": 100
      },
      ""MaxResources"": {
        ""Mana"": 316,
        ""Psy"": 100
      },
      ""Equipments"": [
        {
          ""Slot"": 1,
          ""Item"": {
            ""$type"": ""light"",
            ""TimeLeft"": -1,
            ""ItemId"": 21,
            ""Level"": 0,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": ""Glowing"",
            ""Auras"": []
          }
        },
        {
          ""Slot"": 13,
          ""Item"": {
            ""$type"": ""weapon"",
            ""WeaponFlags"": """",
            ""ItemId"": 3350,
            ""Level"": 16,
            ""DecayPulseLeft"": 0,
            ""ItemFlags"": ""Magic"",
            ""Auras"": [
              {
                ""AbilityName"": ""Envenom"",
                ""Level"": 1,
                ""PulseLeft"": 2108,
                ""AuraFlags"": 2,
                ""Affects"": [
                  {
                    ""$type"": ""weaponFlags"",
                    ""Operator"": 1,
                    ""Modifier"": ""Poison""
                  }
                ]
              }
            ]
          }
        }
      ],
      ""Inventory"": [],
      ""Auras"": [
        {
          ""AbilityName"": ""Giant Strength"",
          ""Level"": 29,
          ""PulseLeft"": 11376,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""characterAttribute"",
              ""Operator"": 0,
              ""Location"": 1,
              ""Modifier"": 4
            }
          ]
        },
        {
          ""AbilityName"": ""Detect Evil"",
          ""Level"": 41,
          ""PulseLeft"": 11572,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""characterFlags"",
              ""Operator"": 1,
              ""Modifier"": ""DetectEvil""
            }
          ]
        },
        {
          ""AbilityName"": ""Detect Good"",
          ""Level"": 39,
          ""PulseLeft"": 11604,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""characterFlags"",
              ""Operator"": 1,
              ""Modifier"": ""DetectGood""
            }
          ]
        },
        {
          ""AbilityName"": ""Detect Invis"",
          ""Level"": 44,
          ""PulseLeft"": 11620,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""characterFlags"",
              ""Operator"": 1,
              ""Modifier"": ""DetectInvis""
            }
          ]
        },
        {
          ""AbilityName"": ""Detect Hidden"",
          ""Level"": 48,
          ""PulseLeft"": 11676,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""characterFlags"",
              ""Operator"": 1,
              ""Modifier"": ""DetectHidden""
            }
          ]
        },
        {
          ""AbilityName"": ""Shock Shield"",
          ""Level"": 48,
          ""PulseLeft"": 1884,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""shieldFlags"",
              ""Operator"": 1,
              ""Modifier"": ""ShockShield""
            }
          ]
        },
        {
          ""AbilityName"": ""Fire Shield"",
          ""Level"": 47,
          ""PulseLeft"": 1900,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""shieldFlags"",
              ""Operator"": 1,
              ""Modifier"": ""FireShield""
            }
          ]
        },
        {
          ""AbilityName"": ""Ice Shield"",
          ""Level"": 49,
          ""PulseLeft"": 1912,
          ""AuraFlags"": 0,
          ""Affects"": [
            {
              ""$type"": ""shieldFlags"",
              ""Operator"": 1,
              ""Modifier"": ""IceShield""
            }
          ]
        }
      ],
      ""CharacterFlags"": ""Haste"",
      ""Immunities"": """",
      ""Resistances"": ""Acid,Bash,Disease,Poison,Slash"",
      ""Vulnerabilities"": ""Cold,Fire,Pierce"",
      ""ShieldFlags"": """",
      ""Attributes"": {
        ""Strength"": 22,
        ""Intelligence"": 22,
        ""Wisdom"": 22,
        ""Dexterity"": 22,
        ""Constitution"": 24,
        ""MaxHitPoints"": 218,
        ""SavingThrow"": 0,
        ""HitRoll"": 0,
        ""DamRoll"": 0,
        ""MaxMovePoints"": 218,
        ""ArmorBash"": 100,
        ""ArmorPierce"": 100,
        ""ArmorSlash"": 100,
        ""ArmorExotic"": 100
      }
    },
    {
      ""CreationTime"": ""2025-11-15T10:58:54.1116913+01:00"",
      ""RoomId"": 3001,
      ""SilverCoins"": 0,
      ""GoldCoins"": 0,
      ""Experience"": 0,
      ""Alignment"": 0,
      ""Trains"": 3,
      ""Practices"": 5,
      ""AutoFlags"": 0,
      ""CurrentQuests"": [],
      ""LearnedAbilities"": [],
      ""Conditions"": {
        ""Full"": 48,
        ""Thirst"": 48,
        ""Hunger"": 48
      },
      ""Aliases"": {},
      ""Cooldowns"": {},
      ""Pets"": [],
      ""Name"": ""toto"",
      ""Race"": ""insectoid"",
      ""Class"": ""druid"",
      ""Level"": 1,
      ""Sex"": 0,
      ""Size"": 2,
      ""HitPoints"": 100,
      ""MovePoints"": 100,
      ""CurrentResources"": {
        ""Mana"": 100,
        ""Psy"": 100
      },
      ""MaxResources"": {
        ""Mana"": 100,
        ""Psy"": 100
      },
      ""Equipments"": [],
      ""Inventory"": [],
      ""Auras"": [],
      ""CharacterFlags"": ""Haste"",
      ""Immunities"": """",
      ""Resistances"": ""Acid,Bash,Disease,Poison,Slash"",
      ""Vulnerabilities"": ""Cold,Fire,Pierce"",
      ""ShieldFlags"": """",
      ""Attributes"": {
        ""Strength"": 16,
        ""Intelligence"": 16,
        ""Wisdom"": 16,
        ""Dexterity"": 16,
        ""Constitution"": 18,
        ""MaxHitPoints"": 100,
        ""SavingThrow"": 0,
        ""HitRoll"": 0,
        ""DamRoll"": 0,
        ""MaxMovePoints"": 100,
        ""ArmorBash"": 100,
        ""ArmorPierce"": 100,
        ""ArmorSlash"": 100,
        ""ArmorExotic"": 100
      }
    }
  ]
}";
    }
}
