using System;
using System.Linq;
using AutoBogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Room;
using Mud.Server.Item;

namespace Mud.Server.Tests
{
    [TestClass]
    public class ItemDeserializationTests : TestBase
    {
        // Armor
        [TestMethod]
        public void ItemData_To_ItemArmor_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemArmorBlueprint armorBlueprint = new ItemArmorBlueprint { Id = 1, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Armor = 150, ArmorKind = ArmorKinds.Mail };
            world.AddItemBlueprint(armorBlueprint);
            ItemData itemData = new ItemData
            {
                ItemId = armorBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItem armor = World.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(armor, typeof(IItemArmor));
            Assert.AreEqual(armorBlueprint.Id, armor.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, armor.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, armor.ItemFlags);
        }

        // Container
        [TestMethod]
        public void ItemData_Empty_To_ItemContainer_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint {Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", ItemCount = 10, WeightMultiplier = 50};
            world.AddItemBlueprint(containerBlueprint);

            ItemContainerData itemData = new ItemContainerData
            {
                ItemId = containerBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItem container = world.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(container, typeof(IItemContainer));
            Assert.AreEqual(containerBlueprint.Id, container.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, container.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, container.ItemFlags);
            Assert.AreEqual(0, (container as IItemContainer).Content.Count());
        }

        [TestMethod]
        public void ItemContainer_OneItem_To_ItemData_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", ItemCount = 10, WeightMultiplier = 50 };
            world.AddItemBlueprint(containerBlueprint);
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint {Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5};
            world.AddItemBlueprint(lightBlueprint);

            ItemContainerData itemData = new ItemContainerData
            {
                ItemId = containerBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Contains = new ItemData[]
                {
                    new ItemData
                    {
                        ItemId = lightBlueprint.Id,
                        DecayPulseLeft = AutoFaker.Generate<int>(),
                        ItemFlags = AutoFaker.Generate<ItemFlags>(),
                    }
                }
            };

            IItem container = world.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(container, typeof(IItemContainer));
            Assert.AreEqual(containerBlueprint.Id, container.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, container.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, container.ItemFlags);
            Assert.AreEqual(1, (container as IItemContainer).Content.Count());
            Assert.AreEqual(lightBlueprint.Id, (container as IItemContainer).Content.First().Blueprint.Id);
            Assert.AreEqual(itemData.Contains[0].DecayPulseLeft, (container as IItemContainer).Content.First().DecayPulseLeft);
        }

        [TestMethod]
        public void ItemData_MultipleItems_To_ItemContainer_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint {Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", ItemCount = 10, WeightMultiplier = 50};
            world.AddItemBlueprint(containerBlueprint);
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint {Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5};
            world.AddItemBlueprint(lightBlueprint);
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint {Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 1};
            world.AddItemBlueprint(portalBlueprint);

            ItemContainerData itemData = new ItemContainerData
            {
                ItemId = containerBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Contains = new ItemData[]
                {
                    new ItemData
                    {
                        ItemId = lightBlueprint.Id,
                        DecayPulseLeft = AutoFaker.Generate<int>(),
                        ItemFlags = AutoFaker.Generate<ItemFlags>(),
                    },
                    new ItemData
                    {
                        ItemId = portalBlueprint.Id,
                        DecayPulseLeft = AutoFaker.Generate<int>(),
                        ItemFlags = AutoFaker.Generate<ItemFlags>(),
                    }
                }
            };

            IItem container = world.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(container, typeof(IItemContainer));
            Assert.AreEqual(containerBlueprint.Id, container.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, container.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, container.ItemFlags);
            Assert.AreEqual(2, (container as IItemContainer).Content.Count());
            Assert.AreEqual(1, (container as IItemContainer).Content.Count(x => x.Blueprint.Id == lightBlueprint.Id));
            Assert.AreEqual(1, (container as IItemContainer).Content.Count(x => x.Blueprint.Id == portalBlueprint.Id));
            Assert.AreEqual(itemData.Contains.Sum(x => x.DecayPulseLeft), (container as IItemContainer).Content.Sum(x => x.DecayPulseLeft));
        }

        [TestMethod]
        public void ItemData_NestedItems_To_ItemContainer_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemContainerBlueprint containerBlueprint1 = new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", ItemCount = 10, WeightMultiplier = 50 };
            world.AddItemBlueprint(containerBlueprint1);
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 };
            world.AddItemBlueprint(lightBlueprint);
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint { Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 1 };
            world.AddItemBlueprint(portalBlueprint);
            ItemContainerBlueprint containerBlueprint2 = new ItemContainerBlueprint {Id = 888, Name = "Container2", ShortDescription = "Container2Short", Description = "Container2Desc", ItemCount = 10, WeightMultiplier = 50};
            world.AddItemBlueprint(containerBlueprint2);
            ItemJewelryBlueprint jewelryBlueprint = new ItemJewelryBlueprint {Id = 3, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc"};
            world.AddItemBlueprint(jewelryBlueprint);
            ItemArmorBlueprint armorBlueprint = new ItemArmorBlueprint {Id = 4, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Armor = 150, ArmorKind = ArmorKinds.Mail};
            world.AddItemBlueprint(armorBlueprint);

            ItemContainerData itemData = new ItemContainerData
            {
                ItemId = containerBlueprint1.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Contains = new ItemData[]
                {
                    new ItemData
                    {
                        ItemId = lightBlueprint.Id,
                        DecayPulseLeft = AutoFaker.Generate<int>(),
                        ItemFlags = AutoFaker.Generate<ItemFlags>(),
                    },
                    new ItemData
                    {
                        ItemId = portalBlueprint.Id,
                        DecayPulseLeft = AutoFaker.Generate<int>(),
                        ItemFlags = AutoFaker.Generate<ItemFlags>(),
                    },
                    new ItemContainerData
                    {
                        ItemId = containerBlueprint2.Id,
                        DecayPulseLeft = AutoFaker.Generate<int>(),
                        ItemFlags = AutoFaker.Generate<ItemFlags>(),
                        Contains = new ItemData[]
                        {
                            new ItemData
                            {
                                ItemId = jewelryBlueprint.Id,
                                DecayPulseLeft = AutoFaker.Generate<int>(),
                                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                            },
                            new ItemData
                            {
                                ItemId = armorBlueprint.Id,
                                DecayPulseLeft = AutoFaker.Generate<int>(),
                                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                            }
                        }
                    },
                }
            };

            IItem container = world.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(container, typeof(IItemContainer));
            Assert.AreEqual(containerBlueprint1.Id, container.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, container.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, container.ItemFlags);
            Assert.AreEqual(3, (container as IItemContainer).Content.Count());
            Assert.AreEqual(1, (container as IItemContainer).Content.Count(x => x.Blueprint.Id == lightBlueprint.Id));
            Assert.AreEqual(1, (container as IItemContainer).Content.Count(x => x.Blueprint.Id == portalBlueprint.Id));
            Assert.AreEqual(1, (container as IItemContainer).Content.Count(x => x.Blueprint.Id == containerBlueprint2.Id));
            IItem nestedContainer = (container as IItemContainer).Content.Single(x => x.Blueprint.Id == containerBlueprint2.Id);
            Assert.IsInstanceOfType(nestedContainer, typeof(IItemContainer));
            Assert.IsNotNull((nestedContainer as IItemContainer).Content);
            Assert.AreEqual(2, (nestedContainer as IItemContainer).Content.Count());
            Assert.AreEqual(1, (nestedContainer as IItemContainer).Content.Count(x => x.Blueprint.Id == jewelryBlueprint.Id));
            Assert.AreEqual(1, (nestedContainer as IItemContainer).Content.Count(x => x.Blueprint.Id == armorBlueprint.Id));
        }

        // Corpse
        [TestMethod]
        public void ItemData_Empty_To_NPCItemCorpse_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint {Id = 1, Name = "room1"}, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint {Id = 999, Name = "Corpse"};
            world.AddItemBlueprint(corpseBlueprint);

            ItemCorpseData itemData = new ItemCorpseData
            {
                ItemId = corpseBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                IsPlayableCharacterCorpse = false,
                CorpseName = "test"
            };

            IItem item = world.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsNotNull(item);
            Assert.IsInstanceOfType(item, typeof(IItemCorpse));
            Assert.IsFalse((item as IItemCorpse).IsPlayableCharacterCorpse);
            Assert.AreEqual(itemData.ItemFlags, item.ItemFlags);
            Assert.AreEqual("corpse of "+ itemData.CorpseName, item.Name);
            Assert.AreEqual(0, (item as IItemCorpse).Content.Count());
            Assert.AreEqual(itemData.DecayPulseLeft, item.DecayPulseLeft);
        }

        [TestMethod]
        public void ItemData_Empty_To_PCItemCorpse_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint { Id = 999, Name = "Corpse" };
            world.AddItemBlueprint(corpseBlueprint);

            ItemCorpseData itemData = new ItemCorpseData
            {
                ItemId = corpseBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                IsPlayableCharacterCorpse = true,
                CorpseName = "test"
            };

            IItem item = world.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsNotNull(item);
            Assert.IsInstanceOfType(item, typeof(IItemCorpse));
            Assert.IsTrue((item as IItemCorpse).IsPlayableCharacterCorpse);
            Assert.AreEqual(itemData.ItemFlags, item.ItemFlags);
            Assert.AreEqual("corpse of " + itemData.CorpseName, item.Name);
            Assert.AreEqual(0, (item as IItemCorpse).Content.Count());
            Assert.AreEqual(itemData.DecayPulseLeft, item.DecayPulseLeft);
        }

        [TestMethod]
        public void ItemData_OneItem_To_NPCItemCorpse_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemCorpseBlueprint corpseBlueprint = new ItemCorpseBlueprint { Id = 999, Name = "Corpse" };
            world.AddItemBlueprint(corpseBlueprint);
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 };
            world.AddItemBlueprint(lightBlueprint);

            ItemCorpseData itemData = new ItemCorpseData
            {
                ItemId = corpseBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                IsPlayableCharacterCorpse = false,
                CorpseName = "test",
                Contains = new ItemData[]
                {
                    new ItemData
                    {
                        ItemId = lightBlueprint.Id,
                        DecayPulseLeft = AutoFaker.Generate<int>(),
                        ItemFlags = AutoFaker.Generate<ItemFlags>(),
                    }, 
                }
            };

            IItem item = world.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsNotNull(item);
            Assert.IsInstanceOfType(item, typeof(IItemCorpse));
            Assert.IsFalse((item as IItemCorpse).IsPlayableCharacterCorpse);
            Assert.AreEqual(itemData.ItemFlags, item.ItemFlags);
            Assert.AreEqual("corpse of " + itemData.CorpseName, item.Name);
            Assert.AreEqual(itemData.DecayPulseLeft, item.DecayPulseLeft);
            Assert.AreEqual(1, (item as IItemCorpse).Content.Count());
            Assert.AreEqual(lightBlueprint.Id, (item as IItemCorpse).Content.First().Blueprint.Id);
            Assert.AreEqual(itemData.Contains[0].DecayPulseLeft, (item as IItemCorpse).Content.First().DecayPulseLeft);
        }

        // Furniture
        [TestMethod]
        public void ItemData_To_ItemFurniture_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemFurnitureBlueprint furnitureBlueprint = new ItemFurnitureBlueprint {Id = 1, Name = "Furniture", ShortDescription = "FurnitureShort", Description = "FurnitureDesc", FurnitureActions = FurnitureActions.Sleep, FurniturePlacePreposition = FurniturePlacePrepositions.On, MaxPeople = 10};
            world.AddItemBlueprint(furnitureBlueprint);

            ItemData itemData = new ItemData
            {
                ItemId = furnitureBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItem furniture = World.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(furniture, typeof(IItemFurniture));
            Assert.AreEqual(furnitureBlueprint.Id, furniture.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, furniture.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, furniture.ItemFlags);
        }

        // Jewelry
        [TestMethod]
        public void ItemData_To_ItemJewelry_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemJewelryBlueprint jewelryBlueprint = new ItemJewelryBlueprint {Id = 1, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc"};
            world.AddItemBlueprint(jewelryBlueprint);

            ItemData itemData = new ItemData
            {
                ItemId = jewelryBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItem jewelry = World.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(jewelry, typeof(IItemJewelry));
            Assert.AreEqual(jewelryBlueprint.Id, jewelry.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, jewelry.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, jewelry.ItemFlags);
        }

        // Key
        [TestMethod]
        public void ItemData_To_ItemKey_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemKeyBlueprint keyBlueprint = new ItemKeyBlueprint {Id = 1, Name = "Key", ShortDescription = "KeyShort", Description = "KeyDesc"};
            world.AddItemBlueprint(keyBlueprint);

            ItemData itemData = new ItemData
            {
                ItemId = keyBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItem key = World.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(key, typeof(IItemKey));
            Assert.AreEqual(keyBlueprint.Id, key.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, key.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, key.ItemFlags);
        }

        // Light
        [TestMethod]
        public void ItemData_To_ItemLight_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint {Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5};
            world.AddItemBlueprint(lightBlueprint);

            ItemData itemData = new ItemData
            {
                ItemId = lightBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItem light = World.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(light, typeof(IItemLight));
            Assert.AreEqual(lightBlueprint.Id, light.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, light.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, light.ItemFlags);
        }

        // Portal
        [TestMethod]
        public void ItemData_To_ItemPortal_Test()
        {
            IWorld world = World;
            IRoom room1 = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IRoom room2 = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 2, Name = "room2" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint {Id = 1, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 2};
            world.AddItemBlueprint(portalBlueprint);

            ItemData itemData = new ItemData
            {
                ItemId = portalBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItem portal = World.AddItem(Guid.NewGuid(), itemData, room1);

            Assert.IsInstanceOfType(portal, typeof(IItemPortal));
            Assert.AreEqual(portalBlueprint.Id, portal.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, portal.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, portal.ItemFlags);
            Assert.IsNotNull((portal as IItemPortal).Destination);
            Assert.AreEqual(room2, (portal as IItemPortal).Destination);
        }

        // Quest
        [TestMethod]
        public void ItemData_To_ItemQuest_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemQuestBlueprint questBlueprint = new ItemQuestBlueprint {Id = 1, Name = "Quest", ShortDescription = "QuestShort", Description = "QuestDesc"};
            world.AddItemBlueprint(questBlueprint);

            ItemData itemData = new ItemData
            {
                ItemId = questBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItem quest = World.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(quest, typeof(IItemQuest));
            Assert.AreEqual(questBlueprint.Id, quest.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, quest.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, quest.ItemFlags);
        }

        // Shield
        [TestMethod]
        public void ItemData_To_ItemShield_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemShieldBlueprint shieldBlueprint = new ItemShieldBlueprint {Id = 1, Name = "Shield", ShortDescription = "ShieldShort", Description = "ShieldDesc", Armor = 150};
            world.AddItemBlueprint(shieldBlueprint);

            ItemData itemData = new ItemData
            {
                ItemId = shieldBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItem shield = World.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(shield, typeof(IItemShield));
            Assert.AreEqual(shieldBlueprint.Id, shield.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, shield.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, shield.ItemFlags);
        }

        // Weapon
        [TestMethod]
        public void ItemData_To_ItemWeapon_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            ItemWeaponBlueprint weaponBlueprint = new ItemWeaponBlueprint { Id = 1, Name = "Weapon", ShortDescription = "WeaponShort", Description = "WeaponDesc", DamageType = SchoolTypes.Fire, DiceCount = 10, DiceValue = 20 };
            world.AddItemBlueprint(weaponBlueprint);

            ItemData itemData = new ItemData
            {
                ItemId = weaponBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItem weapon = World.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(weapon, typeof(IItemWeapon));
            Assert.AreEqual(weaponBlueprint.Id, weapon.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, weapon.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, weapon.ItemFlags);
        }
    }
}
