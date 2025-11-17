using System;
using System.Linq;
using AutoBogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Character.PlayableCharacter;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using Mud.Server.Quest;

namespace Mud.Server.Tests
{
    /*
    [TestClass]
    public class CharacterDeserializationTests : TestBase
    {
        [TestMethod]
        public void PlayableCharacterData_NoEquipmentInventoryQuest_To_PlayableCharacter_Test()
        {
            //AutoFaker cannot be used because BluePrint for each Item/Quest must be created
            //PlayableCharacterData playableCharacterData = AutoFaker.Generate<PlayableCharacterData>();
            PlayableCharacterData playableCharacterData = new PlayableCharacterData
            {
                CreationTime = AutoFaker.Generate<DateTime>(),
                Name = AutoFaker.Generate<string>(),
                RoomId = 1,
                Race = AutoFaker.Generate<string>(), // RaceMock will generate Race at runtime
                Class = AutoFaker.Generate<string>(), // ClassMock will generate Class at runtime
                Level = AutoFaker.Generate<int>(),
                Sex = AutoFaker.Generate<Sex>(),
                Size = AutoFaker.Generate<Sizes>(),
                Experience = AutoFaker.Generate<long>(),
                HitPoints = AutoFaker.Generate<int>(),
                MovePoints = AutoFaker.Generate<int>(),
                CurrentResources = EnumHelpers.GetValues<ResourceKinds>().ToDictionary(x => x, x => (int)AutoFaker.Generate<ushort>()),
                MaxResources = EnumHelpers.GetValues<ResourceKinds>().ToDictionary(x => x, x => (int)AutoFaker.Generate<ushort>()),
                Trains = AutoFaker.Generate<int>(),
                Practices = AutoFaker.Generate<int>(),
                CharacterFlags = new CharacterFlags("Charm", "Calm", "Berserk"),
                Immunities = AutoFaker.Generate<IRVFlags>(),
                Resistances = AutoFaker.Generate<IRVFlags>(),
                Vulnerabilities = AutoFaker.Generate<IRVFlags>(),
                Attributes = EnumHelpers.GetValues<CharacterAttributes>().ToDictionary(x => x, x => (int)AutoFaker.Generate<ushort>()),
                Auras = AutoFaker.Generate<AuraData[]>(),
                LearnedAbilities = AutoFaker.Generate<LearnedAbilityData[]>(), // AbilityManagerMock will generate Ability at runtime // TODO: find a way to automatically (int)AutoFaker.Generate<ushort>() on int fields
                Conditions = EnumHelpers.GetValues<Conditions>().ToDictionary(x => x, x => (int)AutoFaker.Generate<ushort>()),
                Equipments = [],
                Inventory = [],
                CurrentQuests = [],
                Cooldowns = [],
                Aliases = [],
                Pets = [],
                Alignment = AutoFaker.Generate<int>(),
                AutoFlags = AutoFlags.Assist,
                GoldCoins = AutoFaker.Generate<int>(),
                SilverCoins = AutoFaker.Generate<int>(),
            };

            PlayableCharacter playableCharacter = new PlayableCharacter(Guid.NewGuid(), playableCharacterData, new Mock<IPlayer>().Object, new Mock<IRoom>().Object);

            Assert.IsNotNull(playableCharacter);
            Assert.AreEqual(playableCharacterData.CreationTime, playableCharacter.CreationTime);
            Assert.AreEqual(playableCharacterData.Name, playableCharacter.Name);
            // RoomId is only used to retrieve Room in ImpersonateCommand
            Assert.AreEqual(playableCharacterData.Race, playableCharacter.Race.Name);
            Assert.AreEqual(playableCharacterData.Class, playableCharacter.Class.Name);
            Assert.AreEqual(playableCharacterData.Level, playableCharacter.Level);
            Assert.AreEqual(playableCharacterData.Sex, playableCharacter.BaseSex);
            Assert.AreEqual(playableCharacterData.Size, playableCharacter.BaseSize);
            Assert.AreEqual(playableCharacterData.Experience, playableCharacter.Experience);
            Assert.AreEqual(playableCharacterData.HitPoints, playableCharacter.HitPoints);
            Assert.AreEqual(playableCharacterData.MovePoints, playableCharacter.MovePoints);
            Assert.AreEqual(playableCharacterData.CurrentResources[ResourceKinds.Mana], playableCharacter[ResourceKinds.Mana]);
            Assert.AreEqual(playableCharacterData.CurrentResources[ResourceKinds.Psy], playableCharacter[ResourceKinds.Psy]);
            Assert.AreEqual(playableCharacterData.MaxResources[ResourceKinds.Mana], playableCharacter.MaxResource(ResourceKinds.Mana));
            Assert.AreEqual(playableCharacterData.MaxResources[ResourceKinds.Psy], playableCharacter.MaxResource(ResourceKinds.Psy));
            Assert.AreEqual(playableCharacterData.Trains, playableCharacter.Trains);
            Assert.AreEqual(playableCharacterData.Practices, playableCharacter.Practices);
            Assert.IsEmpty(playableCharacter.Equipments.Where(x => x.Item != null));
            Assert.IsEmpty(playableCharacter.Inventory);
            Assert.IsEmpty(playableCharacter.Quests);
            Assert.AreEqual(playableCharacterData.CharacterFlags, playableCharacter.BaseCharacterFlags);
            Assert.AreEqual(playableCharacterData.Immunities, playableCharacter.BaseImmunities);
            Assert.AreEqual(playableCharacterData.Resistances, playableCharacter.BaseResistances);
            Assert.AreEqual(playableCharacterData.Vulnerabilities, playableCharacter.BaseVulnerabilities);
            Assert.AreEqual(playableCharacterData.Attributes.Sum(x => (int)x.Key ^ x.Value), EnumHelpers.GetValues<CharacterAttributes>().Sum(x => (int)x ^ playableCharacter.BaseAttribute(x)));
            Assert.HasCount(playableCharacterData.Auras.Length, playableCharacter.Auras);
            Assert.HasCount(playableCharacterData.Auras.SelectMany(x => x.Affects).Count(), playableCharacter.Auras.SelectMany(x => x.Affects)); // can't test AffectData because AutoFaker doesn't handle abstract class
            Assert.HasCount(playableCharacterData.LearnedAbilities.Length, playableCharacter.LearnedAbilities);
            //Assert.AreEqual(ArithmeticOverflow!!!
            //    playableCharacterData.LearnedAbilities.Sum(x => (int)x.AbilityId ^ (int)x.ResourceKind ^ (int)x.CostAmount ^ (int)x.CostAmountOperator ^ (int)x.Learned ^ (int)x.Level ^ (int)x.Rating),
            //    playableCharacter.LearnedAbilities.Sum(x => (int)x.Ability.Id ^ (int)x.ResourceKind ^ (int)x.CostAmount ^ (int)x.CostAmountOperator ^ (int)x.Learned ^ (int)x.Level ^ (int)x.Rating));
            Assert.AreEqual(playableCharacterData.LearnedAbilities.OrderBy(x => x.Name).First().Name, playableCharacter.LearnedAbilities.OrderBy(x => x.Name).First().Name);
            Assert.AreEqual(playableCharacterData.LearnedAbilities.OrderBy(x => x.Name).First().ResourceKind, playableCharacter.LearnedAbilities.OrderBy(x => x.Name).First().ResourceKind);
            Assert.AreEqual(playableCharacterData.LearnedAbilities.OrderBy(x => x.Name).First().CostAmount, playableCharacter.LearnedAbilities.OrderBy(x => x.Name).First().CostAmount);
            Assert.AreEqual(playableCharacterData.LearnedAbilities.OrderBy(x => x.Name).First().CostAmountOperator, playableCharacter.LearnedAbilities.OrderBy(x => x.Name).First().CostAmountOperator);
            Assert.AreEqual(playableCharacterData.LearnedAbilities.OrderBy(x => x.Name).First().Learned, playableCharacter.LearnedAbilities.OrderBy(x => x.Name).First().Learned);
            Assert.AreEqual(playableCharacterData.LearnedAbilities.OrderBy(x => x.Name).First().Level, playableCharacter.LearnedAbilities.OrderBy(x => x.Name).First().Level);
            Assert.AreEqual(playableCharacterData.LearnedAbilities.OrderBy(x => x.Name).First().Rating, playableCharacter.LearnedAbilities.OrderBy(x => x.Name).First().Rating);
            Assert.AreEqual(playableCharacterData.Conditions.Sum(x => (int)x.Key ^ x.Value), EnumHelpers.GetValues<Conditions>().Sum(x => (int)x ^ playableCharacter[x]));
        }

        [TestMethod]
        public void PlayableCharacterData_Inventory_To_PlayableCharacter_Test()
        {
            // TODO: Can't mock IWorld because World.AddItem is used when deserializing inventory
            var world = World;
            ItemContainerBlueprint containerBlueprint1 = new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", MaxWeight = 100, WeightMultiplier = 50 };
            ItemManager.AddItemBlueprint(containerBlueprint1);
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 };
            ItemManager.AddItemBlueprint(lightBlueprint);
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint { Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 1 };
            ItemManager.AddItemBlueprint(portalBlueprint);
            ItemContainerBlueprint containerBlueprint2 = new ItemContainerBlueprint { Id = 888, Name = "Container2", ShortDescription = "Container2Short", Description = "Container2Desc", MaxWeight = 100, WeightMultiplier = 50 };
            ItemManager.AddItemBlueprint(containerBlueprint2);
            ItemJewelryBlueprint jewelryBlueprint = new ItemJewelryBlueprint { Id = 3, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc" };
            ItemManager.AddItemBlueprint(jewelryBlueprint);
            ItemArmorBlueprint armorBlueprint = new ItemArmorBlueprint { Id = 4, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Bash = 150 };
            ItemManager.AddItemBlueprint(armorBlueprint);

            //AutoFaker cannot be used because BluePrint for each Item/Quest must be created
            //PlayableCharacterData playableCharacterData = AutoFaker.Generate<CharacterData>();
            PlayableCharacterData playableCharacterData = new PlayableCharacterData
            {
                CreationTime = AutoFaker.Generate<DateTime>(),
                Name = AutoFaker.Generate<string>(),
                RoomId = 1,//room.Blueprint.Id,
                Race = AutoFaker.Generate<string>(), // RaceMock will generate Race at runtime
                Class = AutoFaker.Generate<string>(), // ClassMock will generate Class at runtime
                Level = AutoFaker.Generate<int>(),
                Sex = AutoFaker.Generate<Sex>(),
                Experience = AutoFaker.Generate<long>(),
                Inventory = new ItemData[]
                {
                    new ItemLightData
                    {
                        ItemId = lightBlueprint.Id,
                        DecayPulseLeft = 20,
                        Level = 1,
                        ItemFlags = new ItemFlags(),
                        Auras = [],
                        TimeLeft = 10,
                    },
                    new ItemPortalData
                    {
                        ItemId = portalBlueprint.Id,
                        DecayPulseLeft = 30,
                        Auras = [],
                        CurrentChargeCount = 5,
                        ItemFlags = new ItemFlags(),
                        Level = 1,
                        DestinationRoomId = 1,
                        MaxChargeCount = 10,
                        PortalFlags = new PortalFlags(),
                    },
                    new ItemContainerData
                    {
                        ItemId = containerBlueprint2.Id,
                        DecayPulseLeft = 40,
                        Contains = new ItemData[]
                        {
                            new ItemData
                            {
                                ItemId = jewelryBlueprint.Id,
                                DecayPulseLeft = 50,
                                Auras = [],
                                ItemFlags = new ItemFlags(),
                                Level = 1
                            },
                            new ItemData
                            {
                                ItemId = armorBlueprint.Id,
                                DecayPulseLeft = 60,
                                Auras = [],
                                ItemFlags = new ItemFlags(),
                                Level = 1
                            }
                        },
                        Auras = [],
                        ItemFlags = new ItemFlags(),
                        Level = 1,
                        ContainerFlags = new ContainerFlags(),
                        MaxWeight = 100,
                        MaxWeightPerItem = 50,
                    },
                },
                Attributes = [],
                Auras = [],
                CharacterFlags = new CharacterFlags(),
                CurrentResources = [],
                MaxResources = [],
                HitPoints = 0,
                MovePoints = 0,
                Conditions = [],
                Immunities = new IRVFlags(),
                Vulnerabilities = new IRVFlags(),
                Resistances = new IRVFlags(),
                LearnedAbilities = [],
                Size = Sizes.Medium,
                Trains = 0,
                Practices = 0,
                Equipments = [],
                CurrentQuests = [],
                Cooldowns = [],
                Aliases = [],
                Pets = [],
                Alignment = AutoFaker.Generate<int>(),
                AutoFlags = AutoFlags.Assist,
                GoldCoins = AutoFaker.Generate<int>(),
                SilverCoins = AutoFaker.Generate<int>(),
            };

            PlayableCharacter playableCharacter = new PlayableCharacter(Guid.NewGuid(), playableCharacterData, new Mock<IPlayer>().Object, new Mock<IRoom>().Object);

            Assert.IsNotNull(playableCharacter);
            Assert.AreEqual(playableCharacterData.CreationTime, playableCharacter.CreationTime);
            Assert.AreEqual(playableCharacterData.Name, playableCharacter.Name);
            // RoomId is only used to retrieve Room in ImpersonateCommand
            Assert.AreEqual(playableCharacterData.Race, playableCharacter.Race.Name);
            Assert.AreEqual(playableCharacterData.Class, playableCharacter.Class.Name);
            Assert.AreEqual(playableCharacterData.Level, playableCharacter.Level);
            Assert.AreEqual(playableCharacterData.Sex, playableCharacter.BaseSex);
            Assert.AreEqual(playableCharacterData.Experience, playableCharacter.Experience);
            Assert.IsEmpty(playableCharacter.Equipments.Where(x => x.Item != null));
            Assert.IsEmpty(playableCharacter.Quests);
            Assert.HasCount(playableCharacterData.Inventory.Length, playableCharacter.Inventory);
            Assert.ContainsSingle(playableCharacter.Inventory.Where(x => x.Blueprint.Id == lightBlueprint.Id));
            Assert.ContainsSingle(playableCharacter.Inventory.Where(x => x.Blueprint.Id == portalBlueprint.Id));
            Assert.ContainsSingle(playableCharacter.Inventory.Where(x => x.Blueprint.Id == containerBlueprint2.Id));
            IItem container = playableCharacter.Inventory.Single(x => x.Blueprint.Id == containerBlueprint2.Id);
            Assert.IsInstanceOfType(container, typeof(IItemContainer));
            Assert.IsNotNull((container as IItemContainer).Content);
            Assert.HasCount(2, (container as IItemContainer).Content);
            Assert.ContainsSingle((container as IItemContainer).Content.Where(x => x.Blueprint.Id == jewelryBlueprint.Id));
            Assert.ContainsSingle((container as IItemContainer).Content.Where(x => x.Blueprint.Id == armorBlueprint.Id));
        }

        [TestMethod]
        public void PlayableCharacterData_Equipment_To_PlayableCharacter_Test()
        {
            // TODO: Can't mock IWorld because World.AddItem is used when deserializing equipment
            var world = World;
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5, WearLocation = WearLocations.Light};
            ItemManager.AddItemBlueprint(lightBlueprint);
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint { Id = 888, Name = "Container2", ShortDescription = "Container2Short", Description = "Container2Desc", MaxWeight = 100, WeightMultiplier = 50, WearLocation = WearLocations.Hold};
            ItemManager.AddItemBlueprint(containerBlueprint);
            ItemJewelryBlueprint jewelryBlueprint = new ItemJewelryBlueprint { Id = 3, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc", WearLocation = WearLocations.Ring};
            ItemManager.AddItemBlueprint(jewelryBlueprint);
            ItemArmorBlueprint armorBlueprint = new ItemArmorBlueprint { Id = 4, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Bash = 150, WearLocation = WearLocations.Chest};
            ItemManager.AddItemBlueprint(armorBlueprint);
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint { Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 1 };
            ItemManager.AddItemBlueprint(portalBlueprint);

            //AutoFaker cannot be used because BluePrint for each Item/Quest must be created
            //PlayableCharacterData playableCharacterData = AutoFaker.Generate<CharacterData>();
            PlayableCharacterData playableCharacterData = new PlayableCharacterData
            {
                CreationTime = AutoFaker.Generate<DateTime>(),
                Name = AutoFaker.Generate<string>(),
                RoomId = 1,
                Race = AutoFaker.Generate<string>(), // RaceMock will generate Race at runtime with 2 equipmentslots of each kind
                Class = AutoFaker.Generate<string>(), // ClassMock will generate Class at runtime
                Level = AutoFaker.Generate<int>(),
                Sex = AutoFaker.Generate<Sex>(),
                Experience = AutoFaker.Generate<long>(),
                Equipments = new EquippedItemData[]
                {
                    new EquippedItemData
                    {
                        Slot = EquipmentSlots.Light,
                        Item = new ItemLightData
                        {
                            ItemId = lightBlueprint.Id,
                            DecayPulseLeft = AutoFaker.Generate<int>(),
                            ItemFlags = new ItemFlags("Bless"),
                            Auras = [],
                            Level = 1,
                            TimeLeft = 100,
                        },
                    },
                    new EquippedItemData
                    {
                        Slot = EquipmentSlots.OffHand,
                        Item = new ItemContainerData
                        {
                            ItemId = containerBlueprint.Id,
                            DecayPulseLeft = AutoFaker.Generate<int>(),
                            ItemFlags = new ItemFlags("Bless"),
                            Contains = new ItemData[]
                            {
                                new ItemPortalData
                                {
                                    ItemId = portalBlueprint.Id,
                                    DecayPulseLeft = AutoFaker.Generate<int>(),
                                    ItemFlags = new ItemFlags("Bless"),
                                    Auras = [],
                                    Level = 1,
                                    CurrentChargeCount = 5,
                                    DestinationRoomId = 1,
                                    MaxChargeCount = 10,
                                    PortalFlags = PortalFlags.None
                                }
                            },
                            Auras = [],
                            Level = 1,
                            ContainerFlags = new ContainerFlags(),
                            MaxWeight = 100,
                            MaxWeightPerItem = 50
                        },
                    },
                    new EquippedItemData
                    {
                        Slot = EquipmentSlots.Ring,
                        Item = new ItemData
                        {
                            ItemId = jewelryBlueprint.Id,
                            DecayPulseLeft = AutoFaker.Generate<int>(),
                            ItemFlags = new ItemFlags("Bless"),
                            Auras = [],
                            Level = 1
                        },
                    },
                    new EquippedItemData
                    {
                        Slot = EquipmentSlots.Chest,
                        Item = new ItemData
                        {
                            ItemId = armorBlueprint.Id,
                            DecayPulseLeft = AutoFaker.Generate<int>(),
                            ItemFlags = new ItemFlags("Bless"),
                            Auras = [],
                            Level = 1
                        },
                    },
                },
                Attributes = [],
                Auras = [],
                CharacterFlags = new CharacterFlags(),
                CurrentResources = [],
                MaxResources = [],
                HitPoints = 0,
                MovePoints = 0,
                Conditions = [],
                Immunities = new IRVFlags(),
                Vulnerabilities = new IRVFlags(),
                Resistances = new IRVFlags(),
                LearnedAbilities = [],
                Size = Sizes.Medium,
                Trains = 0,
                Practices = 0,
                Inventory = [],
                CurrentQuests = [],
                Cooldowns = [],
                Aliases = [],
                Pets = [],
                Alignment = AutoFaker.Generate<int>(),
                AutoFlags = AutoFlags.Assist,
                GoldCoins = AutoFaker.Generate<int>(),
                SilverCoins = AutoFaker.Generate<int>(),
            };

            PlayableCharacter playableCharacter = new PlayableCharacter(Guid.NewGuid(), playableCharacterData, new Mock<IPlayer>().Object, new Mock<IRoom>().Object);

            Assert.IsNotNull(playableCharacter);
            Assert.AreEqual(playableCharacterData.CreationTime, playableCharacter.CreationTime);
            Assert.AreEqual(playableCharacterData.Name, playableCharacter.Name);
            // RoomId is only used to retrieve Room in ImpersonateCommand
            Assert.AreEqual(playableCharacterData.Race, playableCharacter.Race.Name);
            Assert.AreEqual(playableCharacterData.Class, playableCharacter.Class.Name);
            Assert.AreEqual(playableCharacterData.Level, playableCharacter.Level);
            Assert.AreEqual(playableCharacterData.Sex, playableCharacter.BaseSex);
            Assert.AreEqual(playableCharacterData.Experience, playableCharacter.Experience);
            Assert.IsEmpty(playableCharacter.Quests);
            Assert.IsEmpty( playableCharacter.Inventory);
            Assert.HasCount(playableCharacterData.Equipments.Length, playableCharacter.Equipments.Where(x => x.Item != null));
            Assert.IsNotNull(playableCharacter.Equipments.First(x => x.Slot == EquipmentSlots.Light).Item);
            Assert.IsNotNull(playableCharacter.Equipments.First(x => x.Slot == EquipmentSlots.OffHand).Item);
            Assert.IsNotNull(playableCharacter.Equipments.First(x => x.Slot == EquipmentSlots.Ring).Item);
            Assert.IsNotNull(playableCharacter.Equipments.First(x => x.Slot == EquipmentSlots.Chest).Item);
            Assert.AreEqual(lightBlueprint.Id, playableCharacter.Equipments.First(x => x.Slot == EquipmentSlots.Light).Item.Blueprint.Id);
            Assert.AreEqual(containerBlueprint.Id, playableCharacter.Equipments.First(x => x.Slot == EquipmentSlots.OffHand).Item.Blueprint.Id);
            Assert.AreEqual(jewelryBlueprint.Id, playableCharacter.Equipments.First(x => x.Slot == EquipmentSlots.Ring).Item.Blueprint.Id);
            Assert.AreEqual(armorBlueprint.Id, playableCharacter.Equipments.First(x => x.Slot == EquipmentSlots.Chest).Item.Blueprint.Id);
            Assert.IsInstanceOfType(playableCharacter.Equipments.First(x => x.Slot == EquipmentSlots.OffHand).Item, typeof(IItemContainer));
            IItemContainer container = playableCharacter.Equipments.First(x => x.Slot == EquipmentSlots.OffHand).Item as IItemContainer;
            Assert.ContainsSingle(container.Content);
            Assert.AreEqual(portalBlueprint.Id, container.Content.First().Blueprint.Id);
        }

        [TestMethod]
        public void PlayableCharacterData_Quest_To_PlayableCharacter_Test()
        {
            var world = World;
            RoomBlueprint roomBlueprint = new RoomBlueprint {Id = 1, Name = "room1"};
            RoomManager.AddRoomBlueprint(roomBlueprint);
            IRoom room = RoomManager.AddRoom(Guid.NewGuid(), roomBlueprint, new Mock<IArea>().Object);
            CharacterNormalBlueprint mobBlueprint = new CharacterNormalBlueprint { Id = 1, Name = "mob1", ShortDescription = "Mob1Short", Description = "Mob1Desc" };
            CharacterManager.AddCharacterBlueprint(mobBlueprint);
            ItemQuestBlueprint questItemBlueprint = new ItemQuestBlueprint { Id = 1, Name="item1", ShortDescription = "Item1Short", Description = "Item1Desc"};
            ItemManager.AddItemBlueprint(questItemBlueprint);
            QuestBlueprint questBlueprint1 = new QuestBlueprint
            {
                Id = 1,
                TimeLimit = 10,
                Experience = 1000,
                Gold = 20000,
                Title = "Quest1",
                Level = 20,
                ItemObjectives = new []
                {
                    new QuestItemObjectiveBlueprint
                    {
                        Id = 1,
                        ItemBlueprintId = questItemBlueprint.Id,
                        Count = 5
                    }
                },
                KillObjectives = new []
                {
                    new QuestKillObjectiveBlueprint
                    {
                        Id = 2,
                        CharacterBlueprintId = mobBlueprint.Id,
                        Count = 3
                    }
                },
                LocationObjectives = new []
                {
                    new QuestLocationObjectiveBlueprint
                    {
                        Id = 3,
                        RoomBlueprintId = room.Blueprint.Id
                    }
                }
            };
            QuestManager.AddQuestBlueprint(questBlueprint1);
            CharacterQuestorBlueprint questorBlueprint = new CharacterQuestorBlueprint
            {
                Id = 999, Name = "Questor", ShortDescription = "QuestorShort", Description = "QuestDesc",
                QuestBlueprints = new [] { questBlueprint1 },
            };
            CharacterManager.AddCharacterBlueprint(questorBlueprint);
            CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), questorBlueprint, room);

            //AutoFaker cannot be used because BluePrint for each Item/Quest must be created
            //PlayableCharacterData playableCharacterData = AutoFaker.Generate<CharacterData>();
            PlayableCharacterData playableCharacterData = new PlayableCharacterData
            {
                CreationTime = AutoFaker.Generate<DateTime>(),
                Name = AutoFaker.Generate<string>(),
                RoomId = room.Blueprint.Id,
                Race = AutoFaker.Generate<string>(), // RaceMock will generate Race at runtime
                Class = AutoFaker.Generate<string>(), // ClassMock will generate Class at runtime
                Level = AutoFaker.Generate<int>(),
                Sex = AutoFaker.Generate<Sex>(),
                Experience = AutoFaker.Generate<long>(),
                CurrentQuests = new []
                {
                    new CurrentQuestData
                    {
                        QuestId = questBlueprint1.Id,
                        StartTime = AutoFaker.Generate<DateTime>(),
                        PulseLeft = AutoFaker.Generate<int>(),
                        CompletionTime = null,
                        GiverId = questorBlueprint.Id,
                        GiverRoomId = room.Blueprint.Id,
                        Objectives = new []
                        {
                            new CurrentQuestObjectiveData
                            {
                                ObjectiveId = 1,
                                Count = 2
                            },
                            new CurrentQuestObjectiveData
                            {
                                ObjectiveId = 3,
                                Count = 1
                            }
                        }
                    }, 
                },
                Attributes = [],
                Auras = [],
                CharacterFlags = new CharacterFlags(),
                CurrentResources = [],
                MaxResources = [],
                HitPoints = 0,
                MovePoints = 0,
                Conditions = [],
                Immunities = new IRVFlags(),
                Vulnerabilities = new IRVFlags(),
                Resistances = new IRVFlags(),
                LearnedAbilities = [],
                Size = Sizes.Medium,
                Trains = 0,
                Practices = 0,
                Inventory = [],
                Equipments = [],
                Cooldowns = [],
                Aliases = [],
                Pets = [],
                Alignment = AutoFaker.Generate<int>(),
                AutoFlags = AutoFlags.Assist,
                GoldCoins = AutoFaker.Generate<int>(),
                SilverCoins = AutoFaker.Generate<int>(),
            };

            PlayableCharacter playableCharacter = new PlayableCharacter(Guid.NewGuid(), playableCharacterData, new Mock<IPlayer>().Object, room);

            Assert.IsNotNull(playableCharacter);
            Assert.AreEqual(playableCharacterData.CreationTime, playableCharacter.CreationTime);
            Assert.AreEqual(playableCharacterData.Name, playableCharacter.Name);
            // RoomId is only used to retrieve Room in ImpersonateCommand
            Assert.AreEqual(playableCharacterData.Race, playableCharacter.Race.Name);
            Assert.AreEqual(playableCharacterData.Class, playableCharacter.Class.Name);
            Assert.AreEqual(playableCharacterData.Level, playableCharacter.Level);
            Assert.AreEqual(playableCharacterData.Sex, playableCharacter.BaseSex);
            Assert.AreEqual(playableCharacterData.Experience, playableCharacter.Experience);
            Assert.IsEmpty(playableCharacter.Equipments.Where(x => x.Item != null));
            Assert.IsEmpty(playableCharacter.Inventory);
            Assert.ContainsSingle(playableCharacter.Quests);
            Assert.AreEqual(questBlueprint1.Id, playableCharacter.Quests.Single().Blueprint.Id);
            Assert.HasCount(3, playableCharacter.Quests.Single().Objectives);
            Assert.IsFalse(playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 1).IsCompleted); // objective1: 2 item1 on 5
            Assert.IsFalse(playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 2).IsCompleted); // objective2: 0 mob1 on 3
            Assert.IsTrue(playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 3).IsCompleted); // objective3: explored
            Assert.IsInstanceOfType(playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 1), typeof(ItemQuestObjective));
            Assert.IsInstanceOfType(playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 2), typeof(KillQuestObjective));
            Assert.IsInstanceOfType(playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 3), typeof(LocationQuestObjective));
            Assert.AreEqual(2, (playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 1) as ItemQuestObjective).Count);
            Assert.AreEqual(questItemBlueprint.Id, (playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 1) as ItemQuestObjective).Blueprint.Id);
            Assert.AreEqual(0, (playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 2) as KillQuestObjective).Count);
            Assert.AreEqual(mobBlueprint.Id, (playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 2) as KillQuestObjective).Blueprint.Id);
            Assert.IsTrue((playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 3) as LocationQuestObjective).Explored);
            Assert.AreEqual(room.Blueprint.Id, (playableCharacter.Quests.Single().Objectives.Single(x => x.Id == 3) as LocationQuestObjective).Blueprint.Id);
        }
    }
    */
}
