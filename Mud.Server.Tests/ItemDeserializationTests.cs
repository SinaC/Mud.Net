using System;
using System.Linq;
using AutoBogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Room;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.World;
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
            ItemArmorBlueprint armorBlueprint = new ItemArmorBlueprint { Id = 1, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Bash = 150 };
            ItemData itemData = new ItemData
            {
                ItemId = armorBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Level = 10,
            };

            IItemArmor armor = new ItemArmor(Guid.NewGuid(), armorBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(armorBlueprint.Id, armor.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, armor.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, armor.BaseItemFlags);
            Assert.AreEqual(itemData.Level, armor.Level);
        }

        // Jukebox
        [TestMethod]
        public void ItemData_To_ItemJukebox_Test()
        {
            ItemJukeboxBlueprint blueprint = new ItemJukeboxBlueprint { Id = 1, Name = "Jukebox", ShortDescription = "JukeboxShort", Description = "JukeboxDesc" };
            ItemData itemData = new ItemData
            {
                ItemId = blueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Level = 10,
            };

            IItemJukebox jukebox = new ItemJukebox(Guid.NewGuid(), blueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(blueprint.Id, jukebox.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, jukebox.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, jukebox.BaseItemFlags);
            Assert.AreEqual(itemData.Level, jukebox.Level);
        }

        // Map
        [TestMethod]
        public void ItemData_To_ItemMap_Test()
        {
            ItemMapBlueprint blueprint = new ItemMapBlueprint { Id = 1, Name = "Map", ShortDescription = "MapShort", Description = "MapDesc" };
            ItemData itemData = new ItemData
            {
                ItemId = blueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Level = 10,
            };

            IItemMap map = new ItemMap(Guid.NewGuid(), blueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(blueprint.Id, map.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, map.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, map.BaseItemFlags);
            Assert.AreEqual(itemData.Level, map.Level);
        }

        // Clothing
        [TestMethod]
        public void ItemData_To_ItemClothing_Test()
        {
            ItemClothingBlueprint blueprint = new ItemClothingBlueprint { Id = 1, Name = "Clothing", ShortDescription = "ClothingShort", Description = "ClothingDesc" };
            ItemData itemData = new ItemData
            {
                ItemId = blueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Level = 10,
            };

            IItemClothing clothing = new ItemClothing(Guid.NewGuid(), blueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(blueprint.Id, clothing.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, clothing.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, clothing.BaseItemFlags);
            Assert.AreEqual(itemData.Level, clothing.Level);
        }

        // Trash
        [TestMethod]
        public void ItemData_To_ItemTrash_Test()
        {
            ItemTrashBlueprint trashBlueprint = new ItemTrashBlueprint { Id = 1, Name = "Trash", ShortDescription = "TrashShort", Description = "TrashDesc" };
            ItemData itemData = new ItemData
            {
                ItemId = trashBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Level = 10,
            };

            IItemTrash trash = new ItemTrash(Guid.NewGuid(), trashBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(trashBlueprint.Id, trash.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, trash.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, trash.BaseItemFlags);
            Assert.AreEqual(itemData.Level, trash.Level);
        }

        // Gem
        [TestMethod]
        public void ItemData_To_ItemGem_Test()
        {
            ItemGemBlueprint gemBlueprint = new ItemGemBlueprint { Id = 1, Name = "Gem", ShortDescription = "GemShort", Description = "GemDesc" };
            ItemData itemData = new ItemData
            {
                ItemId = gemBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Level = 10,
            };

            IItemGem gem = new ItemGem(Guid.NewGuid(), gemBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(gemBlueprint.Id, gem.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, gem.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, gem.BaseItemFlags);
            Assert.AreEqual(itemData.Level, gem.Level);
        }

        // Boat
        [TestMethod]
        public void ItemData_To_ItemBoat_Test()
        {
            ItemBoatBlueprint boatBlueprint = new ItemBoatBlueprint { Id = 1, Name = "Boat", ShortDescription = "BoatShort", Description = "BoatDesc" };
            ItemData itemData = new ItemData
            {
                ItemId = boatBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Level = 10,
            };

            IItemBoat boat = new ItemBoat(Guid.NewGuid(), boatBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(boatBlueprint.Id, boat.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, boat.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, boat.BaseItemFlags);
            Assert.AreEqual(itemData.Level, boat.Level);
        }

        // Staff
        [TestMethod]
        public void ItemData_To_ItemStaff_Test()
        {
            ItemStaffBlueprint blueprint = new ItemStaffBlueprint
            {
                Id = 1, Name = "Staff", ShortDescription = "StaffShort", Description = "StaffDesc", ItemFlags = ItemFlags.AntiEvil,
                SpellLevel = 20,
                MaxChargeCount = 10,
                CurrentChargeCount = 7,
                AlreadyRecharged = false,
            };
            ItemStaffData itemData = new ItemStaffData
            {
                ItemId = blueprint.Id, DecayPulseLeft = AutoFaker.Generate<int>(), ItemFlags = AutoFaker.Generate<ItemFlags>(), Level = 10,
                MaxChargeCount = 20,
                CurrentChargeCount = 11,
                AlreadyRecharged = true,
            };

            IItemStaff staff = new ItemStaff(Guid.NewGuid(), blueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(itemData.ItemId, staff.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, staff.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, staff.BaseItemFlags);
            Assert.AreEqual(itemData.MaxChargeCount, staff.MaxChargeCount);
            Assert.AreEqual(itemData.CurrentChargeCount, staff.CurrentChargeCount);
            Assert.AreEqual(itemData.AlreadyRecharged, staff.AlreadyRecharged);
        }

        // Wand
        [TestMethod]
        public void ItemData_To_ItemWand_Test()
        {
            ItemWandBlueprint blueprint = new ItemWandBlueprint
            {
                Id = 1, Name = "Wand", ShortDescription = "WandShort", Description = "WandDesc", ItemFlags = ItemFlags.AntiEvil,
                MaxChargeCount = 10,
                CurrentChargeCount = 7,
                AlreadyRecharged = false,
            };
            ItemWandData itemData = new ItemWandData
            {
                ItemId = blueprint.Id, DecayPulseLeft = AutoFaker.Generate<int>(), ItemFlags = AutoFaker.Generate<ItemFlags>(), Level = 10,
                MaxChargeCount = 20,
                CurrentChargeCount = 11,
                AlreadyRecharged = true,
            };

            IItemWand wand = new ItemWand(Guid.NewGuid(), blueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(itemData.ItemId, wand.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, wand.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, wand.BaseItemFlags);
            Assert.AreEqual(itemData.MaxChargeCount, wand.MaxChargeCount);
            Assert.AreEqual(itemData.CurrentChargeCount, wand.CurrentChargeCount);
            Assert.AreEqual(itemData.AlreadyRecharged, wand.AlreadyRecharged);
        }

        // Warpstone
        [TestMethod]
        public void ItemData_To_ItemWarpstone_Test()
        {
            ItemWarpStoneBlueprint warpstoneBlueprint = new ItemWarpStoneBlueprint { Id = 1, Name = "warpstone", ShortDescription = "warpstoneShort", Description = "warpstoneDesc" };
            ItemData itemData = new ItemData
            {
                ItemId = warpstoneBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Level = 10,
            };

            IItemWarpstone warpstone = new ItemWarpstone(Guid.NewGuid(), warpstoneBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(warpstoneBlueprint.Id, warpstone.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, warpstone.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, warpstone.BaseItemFlags);
            Assert.AreEqual(itemData.Level, warpstone.Level);
        }

        // Food
        [TestMethod]
        public void ItemData_To_ItemFood_Test()
        {
            ItemFoodBlueprint foodBlueprint = new ItemFoodBlueprint { Id = 1, Name = "Food", ShortDescription = "FoodShort", Description = "FoodDesc", Cost = 20, HungerHours = 10, FullHours = 20, IsPoisoned = true };
            ItemFoodData itemData = new ItemFoodData
            {
                ItemId = foodBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                FullHours = AutoFaker.Generate<int>(),
                HungerHours = AutoFaker.Generate<int>(),
                IsPoisoned = false,
            };

            IItemFood food = new ItemFood(Guid.NewGuid(), foodBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.IsInstanceOfType(food, typeof(ItemFood));
            Assert.AreEqual(foodBlueprint.Id, food.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, food.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, food.BaseItemFlags);
            Assert.IsFalse((food as IItemFood).IsPoisoned);
            Assert.AreEqual(itemData.FullHours, (food as IItemFood).FullHours);
            Assert.AreEqual(itemData.HungerHours, (food as IItemFood).HungerHours);
        }

        // DrinkContainer
        [TestMethod]
        public void ItemData_To_ItemDrinkContainer_Test()
        {
            ItemDrinkContainerBlueprint drinkContainerBlueprint = new ItemDrinkContainerBlueprint { Id = 1, Name = "Drink", ShortDescription = "DrinkShort", Description = "DrinkDesc", Cost = 10, CurrentLiquidAmount = 100, MaxLiquidAmount = 350, LiquidType = "water" };
            ItemDrinkContainerData itemData = new ItemDrinkContainerData
            {
                ItemId = drinkContainerBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                CurrentLiquidAmount = AutoFaker.Generate<int>(),
                MaxLiquidAmount = AutoFaker.Generate<int>(),
                LiquidName = AutoFaker.Generate<string>(),
                IsPoisoned = AutoFaker.Generate<bool>(),
            };

            IItemDrinkContainer drinkContainer = new ItemDrinkContainer(Guid.NewGuid(), drinkContainerBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(drinkContainerBlueprint.Id, drinkContainer.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, drinkContainer.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, drinkContainer.BaseItemFlags);
            Assert.AreEqual(itemData.CurrentLiquidAmount, drinkContainer.LiquidLeft);
            Assert.AreEqual(itemData.MaxLiquidAmount, drinkContainer.MaxLiquid);
            Assert.AreEqual(itemData.LiquidName, drinkContainer.LiquidName);
            Assert.AreEqual(itemData.IsPoisoned, drinkContainer.IsPoisoned);
        }

        // Container  IWorld is need because container will use IWorld.AddItem when creating content  TODO: find a way to mock this
        [TestMethod]
        public void ItemData_Empty_To_ItemContainer_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint {Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", MaxWeight = 100, WeightMultiplier = 50, ContainerFlags = ContainerFlags.NoLock | ContainerFlags.Closed};
            world.AddItemBlueprint(containerBlueprint);

            ItemContainerData itemData = new ItemContainerData
            {
                ItemId = containerBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                ContainerFlags = AutoFaker.Generate<ContainerFlags>(),
            };

            IItem container = world.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(container, typeof(IItemContainer));
            Assert.AreEqual(containerBlueprint.Id, container.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, container.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, container.BaseItemFlags);
            Assert.AreEqual(0, (container as IItemContainer).Content.Count());
            Assert.AreEqual(itemData.ContainerFlags, (container as IItemContainer).ContainerFlags);
        }

        [TestMethod]
        public void ItemContainer_OneItem_To_ItemData_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", MaxWeight = 100, WeightMultiplier = 50 };
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
                    new ItemLightData
                    {
                        ItemId = lightBlueprint.Id,
                        DecayPulseLeft = AutoFaker.Generate<int>(),
                        ItemFlags = AutoFaker.Generate<ItemFlags>(),
                        TimeLeft = AutoFaker.Generate<int>(),
                    }
                }
            };

            IItem container = world.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(container, typeof(IItemContainer));
            Assert.AreEqual(containerBlueprint.Id, container.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, container.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, container.BaseItemFlags);
            Assert.AreEqual(1, (container as IItemContainer).Content.Count());
            Assert.AreEqual(lightBlueprint.Id, (container as IItemContainer).Content.First().Blueprint.Id);
            Assert.AreEqual(itemData.Contains[0].DecayPulseLeft, (container as IItemContainer).Content.First().DecayPulseLeft);
        }

        [TestMethod]
        public void ItemData_MultipleItems_To_ItemContainer_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint {Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", MaxWeight = 100, WeightMultiplier = 50};
            world.AddItemBlueprint(containerBlueprint);
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint {Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5};
            world.AddItemBlueprint(lightBlueprint);
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint {Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 1};
            world.AddItemBlueprint(portalBlueprint);

            ItemContainerData itemData = new ItemContainerData
            {
                ItemId = containerBlueprint.Id,
                Level = AutoFaker.Generate<int>(),
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Contains = new ItemData[]
                {
                    new ItemLightData
                    {
                        ItemId = lightBlueprint.Id,
                        Level =AutoFaker.Generate<int>(),
                        DecayPulseLeft = AutoFaker.Generate<int>(),
                        ItemFlags = AutoFaker.Generate<ItemFlags>(),
                    },
                    new ItemPortalData
                    {
                        ItemId = portalBlueprint.Id,
                        Level = AutoFaker.Generate<int>(),
                        DecayPulseLeft = AutoFaker.Generate<int>(),
                        ItemFlags = AutoFaker.Generate<ItemFlags>(),
                    }
                }
            };

            IItem container = world.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(container, typeof(IItemContainer));
            Assert.AreEqual(containerBlueprint.Id, container.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, container.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, container.BaseItemFlags);
            Assert.AreEqual(itemData.Level, container.Level);
            Assert.AreEqual(2, (container as IItemContainer).Content.Count());
            Assert.AreEqual(1, (container as IItemContainer).Content.Count(x => x.Blueprint.Id == lightBlueprint.Id));
            Assert.AreEqual(1, (container as IItemContainer).Content.Count(x => x.Blueprint.Id == portalBlueprint.Id));
            Assert.AreEqual(itemData.Contains.Sum(x => x.DecayPulseLeft %100), (container as IItemContainer).Content.Sum(x => x.DecayPulseLeft % 100)); // % avoid overflow
        }

        [TestMethod]
        public void ItemData_NestedItems_To_ItemContainer_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
            ItemContainerBlueprint containerBlueprint1 = new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", MaxWeight = 100, WeightMultiplier = 50 };
            world.AddItemBlueprint(containerBlueprint1);
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 };
            world.AddItemBlueprint(lightBlueprint);
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint { Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 1 };
            world.AddItemBlueprint(portalBlueprint);
            ItemContainerBlueprint containerBlueprint2 = new ItemContainerBlueprint {Id = 888, Name = "Container2", ShortDescription = "Container2Short", Description = "Container2Desc", MaxWeight = 100, WeightMultiplier = 50};
            world.AddItemBlueprint(containerBlueprint2);
            ItemJewelryBlueprint jewelryBlueprint = new ItemJewelryBlueprint {Id = 3, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc"};
            world.AddItemBlueprint(jewelryBlueprint);
            ItemArmorBlueprint armorBlueprint = new ItemArmorBlueprint {Id = 4, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Bash = 150};
            world.AddItemBlueprint(armorBlueprint);

            ItemContainerData itemData = new ItemContainerData
            {
                ItemId = containerBlueprint1.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                Contains = new ItemData[]
                {
                    new ItemLightData
                    {
                        ItemId = lightBlueprint.Id,
                        DecayPulseLeft = AutoFaker.Generate<int>(),
                        ItemFlags = AutoFaker.Generate<ItemFlags>(),
                    },
                    new ItemPortalData
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
            Assert.AreEqual(itemData.ItemFlags, container.BaseItemFlags);
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

        // Corpse  IWorld is need because corpse will use IWorld.AddItem when creating content  TODO: find a way to mock this
        [TestMethod]
        public void ItemData_Empty_To_NPCItemCorpse_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint {Id = 1, Name = "room1"}, new Mock<IArea>().Object);
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
            Assert.AreEqual(itemData.ItemFlags, item.BaseItemFlags);
            Assert.AreEqual("corpse "+ itemData.CorpseName, item.Name);
            Assert.AreEqual(0, (item as IItemCorpse).Content.Count());
            Assert.AreEqual(itemData.DecayPulseLeft, item.DecayPulseLeft);
        }

        [TestMethod]
        public void ItemData_Empty_To_PCItemCorpse_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
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
            Assert.AreEqual(itemData.ItemFlags, item.BaseItemFlags);
            Assert.AreEqual("corpse " + itemData.CorpseName, item.Name);
            Assert.AreEqual(0, (item as IItemCorpse).Content.Count());
            Assert.AreEqual(itemData.DecayPulseLeft, item.DecayPulseLeft);
        }

        [TestMethod]
        public void ItemData_OneItem_To_NPCItemCorpse_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
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
                    new ItemLightData
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
            Assert.AreEqual(itemData.ItemFlags, item.BaseItemFlags);
            Assert.AreEqual("corpse " + itemData.CorpseName, item.Name);
            Assert.AreEqual(itemData.DecayPulseLeft, item.DecayPulseLeft);
            Assert.AreEqual(1, (item as IItemCorpse).Content.Count());
            Assert.AreEqual(lightBlueprint.Id, (item as IItemCorpse).Content.First().Blueprint.Id);
            Assert.AreEqual(itemData.Contains[0].DecayPulseLeft, (item as IItemCorpse).Content.First().DecayPulseLeft);
        }

        // Furniture
        [TestMethod]
        public void ItemData_To_ItemFurniture_Test()
        {
            ItemFurnitureBlueprint furnitureBlueprint = new ItemFurnitureBlueprint {Id = 1, Name = "Furniture", ShortDescription = "FurnitureShort", Description = "FurnitureDesc", FurnitureActions = FurnitureActions.Sleep, FurniturePlacePreposition = FurniturePlacePrepositions.On, MaxPeople = 10};
            ItemData itemData = new ItemData
            {
                ItemId = furnitureBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItemFurniture furniture = new ItemFurniture(Guid.NewGuid(), furnitureBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(furnitureBlueprint.Id, furniture.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, furniture.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, furniture.BaseItemFlags);
        }

        // Jewelry
        [TestMethod]
        public void ItemData_To_ItemJewelry_Test()
        {
            ItemJewelryBlueprint jewelryBlueprint = new ItemJewelryBlueprint {Id = 1, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc"};
            ItemData itemData = new ItemData
            {
                ItemId = jewelryBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItemJewelry jewelry = new ItemJewelry(Guid.NewGuid(), jewelryBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(jewelryBlueprint.Id, jewelry.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, jewelry.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, jewelry.BaseItemFlags);
        }

        // Key
        [TestMethod]
        public void ItemData_To_ItemKey_Test()
        {
            ItemKeyBlueprint keyBlueprint = new ItemKeyBlueprint {Id = 1, Name = "Key", ShortDescription = "KeyShort", Description = "KeyDesc"};
            ItemData itemData = new ItemData
            {
                ItemId = keyBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItemKey key = new ItemKey(Guid.NewGuid(), keyBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(keyBlueprint.Id, key.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, key.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, key.BaseItemFlags);
        }

        // Light
        [TestMethod]
        public void ItemData_To_ItemLight_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
            ItemLightBlueprint lightBlueprint = new ItemLightBlueprint {Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5};
            world.AddItemBlueprint(lightBlueprint);

            ItemLightData itemData = new ItemLightData
            {
                ItemId = lightBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                TimeLeft = AutoFaker.Generate<int>(),
            };

            IItem light = World.AddItem(Guid.NewGuid(), itemData, room);

            Assert.IsInstanceOfType(light, typeof(IItemLight));
            Assert.AreEqual(lightBlueprint.Id, light.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, light.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, light.BaseItemFlags);
            Assert.AreEqual(itemData.TimeLeft, (light as IItemLight).TimeLeft);
        }

        // Portal  IWorld is needed because AddItem will search for destination
        [TestMethod]
        public void ItemData_To_ItemPortal_Test()
        {
            IWorld world = World;
            IRoom room1 = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
            IRoom room2 = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 2, Name = "room2" }, new Mock<IArea>().Object);
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint {Id = 1, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 2};
            world.AddItemBlueprint(portalBlueprint);
            ItemPortalData itemData = new ItemPortalData
            {
                ItemId = portalBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                PortalFlags = AutoFaker.Generate<PortalFlags>(),
                MaxChargeCount = AutoFaker.Generate<int>(),
                CurrentChargeCount = AutoFaker.Generate<int>(),
            };

            IItem portal = World.AddItem(Guid.NewGuid(), itemData, room1);

            Assert.IsInstanceOfType(portal, typeof(IItemPortal));
            Assert.AreEqual(portalBlueprint.Id, portal.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, portal.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, portal.BaseItemFlags);
            Assert.IsNotNull((portal as IItemPortal).Destination);
            Assert.AreEqual(room2, (portal as IItemPortal).Destination);
            Assert.AreEqual(itemData.PortalFlags, (portal as IItemPortal).PortalFlags);
            Assert.AreEqual(itemData.MaxChargeCount, (portal as IItemPortal).MaxChargeCount);
            Assert.AreEqual(itemData.CurrentChargeCount, (portal as IItemPortal).CurrentChargeCount);
        }

        // Quest
        [TestMethod]
        public void ItemData_To_ItemQuest_Test()
        {
            ItemQuestBlueprint questBlueprint = new ItemQuestBlueprint {Id = 1, Name = "Quest", ShortDescription = "QuestShort", Description = "QuestDesc"};
            ItemData itemData = new ItemData
            {
                ItemId = questBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItemQuest quest = new ItemQuest(Guid.NewGuid(), questBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(questBlueprint.Id, quest.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, quest.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, quest.BaseItemFlags);
        }

        // Shield
        [TestMethod]
        public void ItemData_To_ItemShield_Test()
        {
            ItemShieldBlueprint shieldBlueprint = new ItemShieldBlueprint {Id = 1, Name = "Shield", ShortDescription = "ShieldShort", Description = "ShieldDesc", Armor = 150};
            ItemData itemData = new ItemData
            {
                ItemId = shieldBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
            };

            IItemShield shield = new ItemShield(Guid.NewGuid(), shieldBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(shieldBlueprint.Id, shield.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, shield.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, shield.BaseItemFlags);
        }

        // Weapon
        [TestMethod]
        public void ItemData_To_ItemWeapon_Test()
        {
            ItemWeaponBlueprint weaponBlueprint = new ItemWeaponBlueprint { Id = 1, Name = "Weapon", ShortDescription = "WeaponShort", Description = "WeaponDesc", DamageType = SchoolTypes.Fire, DiceCount = 10, DiceValue = 20 };
            ItemWeaponData itemData = new ItemWeaponData
            {
                ItemId = weaponBlueprint.Id,
                DecayPulseLeft = AutoFaker.Generate<int>(),
                ItemFlags = AutoFaker.Generate<ItemFlags>(),
                WeaponFlags = AutoFaker.Generate<WeaponFlags>()
            };

            IItemWeapon weapon = new ItemWeapon(Guid.NewGuid(), weaponBlueprint, itemData, new Mock<IContainer>().Object);

            Assert.AreEqual(weaponBlueprint.Id, weapon.Blueprint.Id);
            Assert.AreEqual(itemData.DecayPulseLeft, weapon.DecayPulseLeft);
            Assert.AreEqual(itemData.ItemFlags, weapon.BaseItemFlags);
            Assert.AreEqual(itemData.WeaponFlags, weapon.BaseWeaponFlags);
        }
    }
}
