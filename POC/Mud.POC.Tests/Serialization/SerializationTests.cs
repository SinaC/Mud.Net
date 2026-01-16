using DeepEqual.Syntax;
using Mud.Domain;
using Mud.POC.Serialization;
using Mud.Server.Domain;
using Mud.Server.Flags;
using System.Text.Json;

namespace Mud.POC.Tests.Serialization
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void DedicatedConverter_AffectData() 
        {
            AffectDataJsonConverter converter = new();

            var originalCharacterAdditionalHitAffectData = new CharacterAdditionalHitAffectData { OneProperty = "some value", HitCount = 1 };
            var originalCharacterAttributeAffectData = new CharacterAttributeAffectData { OneProperty = "another value", Location = CharacterAttributeAffectLocations.Strength, Modifier = 97, Operator = AffectOperators.Assign };

            var characterAdditionalHitAffectDataJson = converter.Serialize(originalCharacterAdditionalHitAffectData);
            var characterAttributeAffectDataJson = converter.Serialize(originalCharacterAttributeAffectData);
            var deserializedCharacterAdditionalHitAffectData = converter.Deserialize(characterAdditionalHitAffectDataJson);
            var deserializedCharacterAttributeAffectData = converter.Deserialize(characterAttributeAffectDataJson);

            deserializedCharacterAdditionalHitAffectData.ShouldDeepEqual(originalCharacterAdditionalHitAffectData);
            deserializedCharacterAttributeAffectData.ShouldDeepEqual(originalCharacterAttributeAffectData);
            Assert.IsInstanceOfType(deserializedCharacterAdditionalHitAffectData, typeof(CharacterAdditionalHitAffectData));
            Assert.IsInstanceOfType(deserializedCharacterAttributeAffectData, typeof(CharacterAttributeAffectData));
        }

        [TestMethod]
        public void GenericConverter_AffectData()
        {
            DiscriminatorJsonConverter converter = new();

            var originalCharacterAdditionalHitAffectData = new CharacterAdditionalHitAffectData { OneProperty = "some value", HitCount = 1 };
            var originalCharacterAttributeAffectData = new CharacterAttributeAffectData { OneProperty = "another value", Location = CharacterAttributeAffectLocations.Strength, Modifier = 97, Operator = AffectOperators.Assign };

            var characterAdditionalHitAffectDataJson = converter.Serialize(originalCharacterAdditionalHitAffectData);
            var characterAttributeAffectDataJson = converter.Serialize(originalCharacterAttributeAffectData);
            var deserializedCharacterAdditionalHitAffectData = converter.Deserialize(characterAdditionalHitAffectDataJson);
            var deserializedCharacterAttributeAffectData = converter.Deserialize(characterAttributeAffectDataJson);

            deserializedCharacterAdditionalHitAffectData.ShouldDeepEqual(originalCharacterAdditionalHitAffectData);
            deserializedCharacterAttributeAffectData.ShouldDeepEqual(originalCharacterAttributeAffectData);
            Assert.IsInstanceOfType<CharacterAdditionalHitAffectData>(deserializedCharacterAdditionalHitAffectData);
            Assert.IsInstanceOfType<CharacterAttributeAffectData>(deserializedCharacterAttributeAffectData);
        }

        [TestMethod]
        public void GenericConverter_CharacterData_Fail()
        {
            DiscriminatorJsonConverter converter = new();

            var originalPetData = new PetData
            {
                Name = "pouet",
                Sex = Sex.Neutral,
                CurrentResources = new Dictionary<ResourceKinds, int>
                {
                    { ResourceKinds.Mana, 78 },
                    { ResourceKinds.Psy, 123 }
                },
                AffectDatas =
                [
                    new CharacterAdditionalHitAffectData { OneProperty = "some value", HitCount = 1 },
                    new CharacterAttributeAffectData { OneProperty = "another value", Location = CharacterAttributeAffectLocations.Strength, Modifier = 97, Operator = AffectOperators.Assign }
                ],
                BlueprintId = 15
            };

            var petDataJson = converter.Serialize(originalPetData);
            Assert.Throws<NotSupportedException>(() => converter.Deserialize(petDataJson));
        }

        [TestMethod]
        public void PolymorphicTypeResolver_PetData()
        {
            var originalPetData = new PetData
            {
                Name = "pouet",
                Sex = Sex.Neutral,
                CharacterFlags = new CharacterFlags("haste", "slow", "test").Serialize(),
                CurrentResources = new Dictionary<ResourceKinds, int>
                {
                    { ResourceKinds.Mana, 78 },
                    { ResourceKinds.Psy, 123 }
                },
                AffectDatas =
                [
                    new CharacterAdditionalHitAffectData { OneProperty = "some value", HitCount = 1 },
                    new CharacterAttributeAffectData { OneProperty = "another value", Location = CharacterAttributeAffectLocations.Strength, Modifier = 97, Operator = AffectOperators.Assign }
                ],
                BlueprintId = 15
            };

            var options = GenerateJsonSerializerOptions();
            var petDataJson = JsonSerializer.Serialize(originalPetData, options);
            var deserializedPetData = JsonSerializer.Deserialize<PetData>(petDataJson, options);

            deserializedPetData.ShouldDeepEqual(originalPetData);
            Assert.IsInstanceOfType<PetData>(deserializedPetData);
        }

        [TestMethod]
        public void PolymorphicTypeResolver_PlayerData()
        {
            var originalPlayerData = new PlayerData
            {
                Name = "SinaC",
                Characters =
                [
                    new PlayableCharacterData
                    {
                        Name = "Glouk",
                        CreationTime = DateTime.Now,
                        CharacterFlags = new CharacterFlags("DetectGood", "Poison", "test").Serialize(),
                        CurrentResources =  new Dictionary<ResourceKinds, int>
                        {
                            { ResourceKinds.Rage, 78 },
                            { ResourceKinds.Energy, 123 }
                        },
                        AffectDatas =
                        [
                            new CharacterAdditionalHitAffectData { OneProperty = "some value 1", HitCount = 1 },
                            new CharacterAttributeAffectData { OneProperty = "another value 2", Location = CharacterAttributeAffectLocations.Strength, Modifier = 97, Operator = AffectOperators.Assign }
                        ],
                        Pets =
                        [
                            new PetData
                            {
                                Name = "pouet",
                                Sex = Sex.Neutral,
                                CharacterFlags = new CharacterFlags("haste", "slow", "test", "PassDoor").Serialize(),
                                CurrentResources = new Dictionary<ResourceKinds, int>
                                {
                                    { ResourceKinds.Mana, 78 },
                                    { ResourceKinds.Psy, 123 }
                                },
                                AffectDatas =
                                [
                                    new CharacterAdditionalHitAffectData { OneProperty = "some value 3", HitCount = 1 },
                                    new CharacterAttributeAffectData { OneProperty = "another value 4", Location = CharacterAttributeAffectLocations.Strength, Modifier = 97, Operator = AffectOperators.Assign }
                                ],
                                BlueprintId = 15
                            }
                        ]
                    }
                ]
            };

            var options = GenerateJsonSerializerOptions();
            var playerDataJson = JsonSerializer.Serialize(originalPlayerData, options);
            var deserializedPlayerData = JsonSerializer.Deserialize<PlayerData>(playerDataJson, options);

            deserializedPlayerData.ShouldDeepEqual(originalPlayerData);
        }

        [TestMethod]
        public void PolymorphicTypeResolver_AdminData()
        {
            var originalAdminData = new AdminData
            {
                AdminLevel = AdminLevels.DemiGod,
                Name = "SinaC",
                WiznetFlags = WiznetFlags.Deaths | WiznetFlags.Incarnate,
                Characters =
                [
                    new PlayableCharacterData
                    {
                        Name = "Glouk",
                        CreationTime = DateTime.Now,
                        CharacterFlags = new CharacterFlags("DetectGood", "Poison", "test").Serialize(),
                        CurrentResources =  new Dictionary<ResourceKinds, int>
                        {
                            { ResourceKinds.Rage, 78 },
                            { ResourceKinds.Energy, 123 }
                        },
                        AffectDatas =
                        [
                            new CharacterAdditionalHitAffectData { OneProperty = "some value 1", HitCount = 1 },
                            new CharacterAttributeAffectData { OneProperty = "another value 2", Location = CharacterAttributeAffectLocations.Strength, Modifier = 97, Operator = AffectOperators.Assign }
                        ],
                        Pets =
                        [
                            new PetData
                            {
                                Name = "pouet",
                                Sex = Sex.Neutral,
                                CharacterFlags = new CharacterFlags("haste", "slow", "test", "PassDoor").Serialize(),
                                CurrentResources = new Dictionary<ResourceKinds, int>
                                {
                                    { ResourceKinds.Mana, 78 },
                                    { ResourceKinds.Psy, 123 }
                                },
                                AffectDatas =
                                [
                                    new CharacterAdditionalHitAffectData { OneProperty = "some value 3", HitCount = 1 },
                                    new CharacterAttributeAffectData { OneProperty = "another value 4", Location = CharacterAttributeAffectLocations.Strength, Modifier = 97, Operator = AffectOperators.Assign }
                                ],
                                BlueprintId = 15
                            }
                        ]
                    }
                ]
            };

            var options = GenerateJsonSerializerOptions();
            var adminDataJson = JsonSerializer.Serialize(originalAdminData, options);
            var deserializedAdminData = JsonSerializer.Deserialize<AdminData>(adminDataJson, options);

            deserializedAdminData.ShouldDeepEqual(originalAdminData);
        }

        private JsonSerializerOptions GenerateJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                TypeInfoResolver = new PolymorphicTypeResolver()
            };
            return options;
        }
    }
}
