using System;
using System.Collections.Generic;
using System.Linq;
using AutoBogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Blueprints.Room;
using Mud.Server.Character.PlayableCharacter;
using Mud.Server.Item;
using Mud.Server.Quest;

namespace Mud.Server.Tests
{
    [TestClass]
    public class CharacterDeserializationTests : TestBase
    {
        [TestMethod]
        public void CharacterData_NoEquipmentInventoryQuest_To_PlayableCharacter_Test()
        {
            var world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IPlayer player = new Player.Player(Guid.NewGuid(), "Player");

            //AutoFaker cannot be used because BluePrint for each Item/Quest must be created
            //CharacterData characterData = AutoFaker.Generate<CharacterData>();
            CharacterData characterData = new CharacterData
            {
                CreationTime = AutoFaker.Generate<DateTime>(),
                Name = AutoFaker.Generate<string>(),
                RoomId = room.Blueprint.Id,
                Race = AutoFaker.Generate<string>(), // RaceMock will generate Race at runtime
                Class = AutoFaker.Generate<string>(), // ClassMock will generate Class at runtime
                Level = AutoFaker.Generate<int>(),
                Sex = AutoFaker.Generate<Sex>(),
                Experience = AutoFaker.Generate<long>()
            };

            PlayableCharacter playableCharacter = new PlayableCharacter(Guid.NewGuid(), characterData, player, room);

            Assert.IsNotNull(playableCharacter);
            Assert.AreEqual(characterData.CreationTime, playableCharacter.CreationTime);
            Assert.AreEqual(characterData.Name, playableCharacter.Name);
            // RoomId is only used to retrieve Room in ImpersonateCommand
            Assert.AreEqual(characterData.Race, playableCharacter.Race.Name);
            Assert.AreEqual(characterData.Class, playableCharacter.Class.Name);
            Assert.AreEqual(characterData.Level, playableCharacter.Level);
            Assert.AreEqual(characterData.Sex, playableCharacter.Sex);
            Assert.AreEqual(characterData.Experience, playableCharacter.Experience);
            Assert.AreEqual(0, playableCharacter.Equipments.Count(x => x.Item != null));
            Assert.AreEqual(0, playableCharacter.Content.Count());
            Assert.AreEqual(0, playableCharacter.Quests.Count());
        }

        [TestMethod]
        public void CharacterData_Inventory_To_PlayableCharacter_Test()
        {
            var world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IPlayer player = new Player.Player(Guid.NewGuid(), "Player");
            ItemContainerBlueprint containerBlueprint1 = new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", ItemCount = 10, WeightMultiplier = 50 };
            world.AddItemBlueprint(containerBlueprint1);
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 };
            world.AddItemBlueprint(lightBlueprint);
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint { Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 1 };
            world.AddItemBlueprint(portalBlueprint);
            ItemContainerBlueprint containerBlueprint2 = new ItemContainerBlueprint { Id = 888, Name = "Container2", ShortDescription = "Container2Short", Description = "Container2Desc", ItemCount = 10, WeightMultiplier = 50 };
            world.AddItemBlueprint(containerBlueprint2);
            ItemJewelryBlueprint jewelryBlueprint = new ItemJewelryBlueprint { Id = 3, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc" };
            world.AddItemBlueprint(jewelryBlueprint);
            ItemArmorBlueprint armorBlueprint = new ItemArmorBlueprint { Id = 4, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Armor = 150, ArmorKind = ArmorKinds.Mail };
            world.AddItemBlueprint(armorBlueprint);

            //AutoFaker cannot be used because BluePrint for each Item/Quest must be created
            //CharacterData characterData = AutoFaker.Generate<CharacterData>();
            CharacterData characterData = new CharacterData
            {
                CreationTime = AutoFaker.Generate<DateTime>(),
                Name = AutoFaker.Generate<string>(),
                RoomId = room.Blueprint.Id,
                Race = AutoFaker.Generate<string>(), // RaceMock will generate Race at runtime
                Class = AutoFaker.Generate<string>(), // ClassMock will generate Class at runtime
                Level = AutoFaker.Generate<int>(),
                Sex = AutoFaker.Generate<Sex>(),
                Experience = AutoFaker.Generate<long>(),
                Inventory = new ItemData[]
                {
                    new ItemData
                    {
                        ItemId = lightBlueprint.Id,
                        DecayPulseLeft = 20
                    },
                    new ItemData
                    {
                        ItemId = portalBlueprint.Id,
                        DecayPulseLeft = 30
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
                                DecayPulseLeft = 50
                            },
                            new ItemData
                            {
                                ItemId = armorBlueprint.Id,
                                DecayPulseLeft = 60
                            }
                        }
                    },
                }
            };

            PlayableCharacter playableCharacter = new PlayableCharacter(Guid.NewGuid(), characterData, player, room);

            Assert.IsNotNull(playableCharacter);
            Assert.AreEqual(characterData.CreationTime, playableCharacter.CreationTime);
            Assert.AreEqual(characterData.Name, playableCharacter.Name);
            // RoomId is only used to retrieve Room in ImpersonateCommand
            Assert.AreEqual(characterData.Race, playableCharacter.Race.Name);
            Assert.AreEqual(characterData.Class, playableCharacter.Class.Name);
            Assert.AreEqual(characterData.Level, playableCharacter.Level);
            Assert.AreEqual(characterData.Sex, playableCharacter.Sex);
            Assert.AreEqual(characterData.Experience, playableCharacter.Experience);
            Assert.AreEqual(0, playableCharacter.Equipments.Count(x => x.Item != null));
            Assert.AreEqual(0, playableCharacter.Quests.Count());
            Assert.AreEqual(characterData.Inventory.Length, playableCharacter.Content.Count());
            Assert.AreEqual(1, playableCharacter.Content.Count(x => x.Blueprint.Id == lightBlueprint.Id));
            Assert.AreEqual(1, playableCharacter.Content.Count(x => x.Blueprint.Id == portalBlueprint.Id));
            Assert.AreEqual(1, playableCharacter.Content.Count(x => x.Blueprint.Id == containerBlueprint2.Id));
            IItem container = playableCharacter.Content.Single(x => x.Blueprint.Id == containerBlueprint2.Id);
            Assert.IsInstanceOfType(container, typeof(IItemContainer));
            Assert.IsNotNull((container as IItemContainer).Content);
            Assert.AreEqual(2, (container as IItemContainer).Content.Count());
            Assert.AreEqual(1, (container as IItemContainer).Content.Count(x => x.Blueprint.Id == jewelryBlueprint.Id));
            Assert.AreEqual(1, (container as IItemContainer).Content.Count(x => x.Blueprint.Id == armorBlueprint.Id));
        }

        [TestMethod]
        public void CharacterData_Equipment_To_PlayableCharacter_Test()
        {
            var world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IPlayer player = new Player.Player(Guid.NewGuid(), "Player");
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5, WearLocation = WearLocations.Light};
            world.AddItemBlueprint(lightBlueprint);
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint { Id = 888, Name = "Container2", ShortDescription = "Container2Short", Description = "Container2Desc", ItemCount = 10, WeightMultiplier = 50, WearLocation = WearLocations.Hold};
            world.AddItemBlueprint(containerBlueprint);
            ItemJewelryBlueprint jewelryBlueprint = new ItemJewelryBlueprint { Id = 3, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc", WearLocation = WearLocations.Ring};
            world.AddItemBlueprint(jewelryBlueprint);
            ItemArmorBlueprint armorBlueprint = new ItemArmorBlueprint { Id = 4, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Armor = 150, ArmorKind = ArmorKinds.Mail, WearLocation = WearLocations.Chest};
            world.AddItemBlueprint(armorBlueprint);
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint { Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 1 };
            world.AddItemBlueprint(portalBlueprint);

            //AutoFaker cannot be used because BluePrint for each Item/Quest must be created
            //CharacterData characterData = AutoFaker.Generate<CharacterData>();
            CharacterData characterData = new CharacterData
            {
                CreationTime = AutoFaker.Generate<DateTime>(),
                Name = AutoFaker.Generate<string>(),
                RoomId = room.Blueprint.Id,
                Race = AutoFaker.Generate<string>(), // RaceMock will generate Race at runtime with 2 equipmentslots of each kind
                Class = AutoFaker.Generate<string>(), // ClassMock will generate Class at runtime
                Level = AutoFaker.Generate<int>(),
                Sex = AutoFaker.Generate<Sex>(),
                Experience = AutoFaker.Generate<long>(),
                Equipments = new EquipedItemData[]
                {
                    new EquipedItemData
                    {
                        Slot = EquipmentSlots.Light,
                        Item = new ItemData
                        {
                            ItemId = lightBlueprint.Id,
                            DecayPulseLeft = 10
                        },
                    },
                    new EquipedItemData
                    {
                        Slot = EquipmentSlots.OffHand,
                        Item = new ItemContainerData
                        {
                            ItemId = containerBlueprint.Id,
                            DecayPulseLeft = 20,
                            Contains = new ItemData[]
                            {
                                new ItemData
                                {
                                    ItemId = portalBlueprint.Id,
                                    DecayPulseLeft = 30
                                }
                            }
                        },
                    },
                    new EquipedItemData
                    {
                        Slot = EquipmentSlots.Ring,
                        Item = new ItemData
                        {
                            ItemId = jewelryBlueprint.Id,
                            DecayPulseLeft = 40
                        },
                    },
                    new EquipedItemData
                    {
                        Slot = EquipmentSlots.Chest,
                        Item = new ItemData
                        {
                            ItemId = armorBlueprint.Id,
                            DecayPulseLeft = 50
                        },
                    },
                },
            };

            PlayableCharacter playableCharacter = new PlayableCharacter(Guid.NewGuid(), characterData, player, room);

            Assert.IsNotNull(playableCharacter);
            Assert.AreEqual(characterData.CreationTime, playableCharacter.CreationTime);
            Assert.AreEqual(characterData.Name, playableCharacter.Name);
            // RoomId is only used to retrieve Room in ImpersonateCommand
            Assert.AreEqual(characterData.Race, playableCharacter.Race.Name);
            Assert.AreEqual(characterData.Class, playableCharacter.Class.Name);
            Assert.AreEqual(characterData.Level, playableCharacter.Level);
            Assert.AreEqual(characterData.Sex, playableCharacter.Sex);
            Assert.AreEqual(characterData.Experience, playableCharacter.Experience);
            Assert.AreEqual(0, playableCharacter.Quests.Count());
            Assert.AreEqual(0 , playableCharacter.Content.Count());
            Assert.AreEqual(characterData.Equipments.Length, playableCharacter.Equipments.Count(x => x.Item != null));
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
            Assert.AreEqual(1, container.Content.Count());
            Assert.AreEqual(portalBlueprint.Id, container.Content.First().Blueprint.Id);
        }

        [TestMethod]
        public void CharacterData_Quest_To_PlayableCharacter_Test()
        {
            var world = World;
            RoomBlueprint roomBlueprint = new RoomBlueprint {Id = 1, Name = "room1"};
            world.AddRoomBlueprint(roomBlueprint);
            IRoom room = world.AddRoom(Guid.NewGuid(), roomBlueprint, new Area.Area("Area", 1, 100, "builders", "credits"));
            IPlayer player = new Player.Player(Guid.NewGuid(), "Player");
            CharacterNormalBlueprint mobBlueprint = new CharacterNormalBlueprint { Id = 1, Name = "mob1", ShortDescription = "Mob1Short", Description = "Mob1Desc" };
            world.AddCharacterBlueprint(mobBlueprint);
            ItemQuestBlueprint questItemBlueprint = new ItemQuestBlueprint { Id = 1, Name="item1", ShortDescription = "Item1Short", Description = "Item1Desc"};
            world.AddItemBlueprint(questItemBlueprint);
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
            world.AddQuestBlueprint(questBlueprint1);
            CharacterQuestorBlueprint questorBlueprint = new CharacterQuestorBlueprint
            {
                Id = 999, Name = "Questor", ShortDescription = "QuestorShort", Description = "QuestDesc",
                QuestBlueprints = new [] { questBlueprint1 },
            };
            world.AddCharacterBlueprint(questorBlueprint);
            world.AddNonPlayableCharacter(Guid.NewGuid(), questorBlueprint, room);

            //AutoFaker cannot be used because BluePrint for each Item/Quest must be created
            //CharacterData characterData = AutoFaker.Generate<CharacterData>();
            CharacterData characterData = new CharacterData
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
                }
            };

            PlayableCharacter playableCharacter = new PlayableCharacter(Guid.NewGuid(), characterData, player, room);

            Assert.IsNotNull(playableCharacter);
            Assert.AreEqual(characterData.CreationTime, playableCharacter.CreationTime);
            Assert.AreEqual(characterData.Name, playableCharacter.Name);
            // RoomId is only used to retrieve Room in ImpersonateCommand
            Assert.AreEqual(characterData.Race, playableCharacter.Race.Name);
            Assert.AreEqual(characterData.Class, playableCharacter.Class.Name);
            Assert.AreEqual(characterData.Level, playableCharacter.Level);
            Assert.AreEqual(characterData.Sex, playableCharacter.Sex);
            Assert.AreEqual(characterData.Experience, playableCharacter.Experience);
            Assert.AreEqual(0, playableCharacter.Equipments.Count(x => x.Item != null));
            Assert.AreEqual(0, playableCharacter.Content.Count());
            Assert.AreEqual(1, playableCharacter.Quests.Count());
            Assert.AreEqual(questBlueprint1.Id, playableCharacter.Quests.Single().Blueprint.Id);
            Assert.AreEqual(3, playableCharacter.Quests.Single().Objectives.Count());
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
}
