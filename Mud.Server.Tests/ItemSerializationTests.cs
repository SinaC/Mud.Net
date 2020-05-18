using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Item;

namespace Mud.Server.Tests
{
    [TestClass]
    public class ItemSerializationTests : TestBase // TODo: remove test base when IRandomManager will be injectable in ItemCorpse
    {
        //public void Test() 
        //{
        // Cannot specify a parameter to transfer to Faker
        //    var faker = new Bogus.Faker<ItemData>()
        //        .RuleFor(x => x.DecayPulseLeft, x => x.Random.Int())
        //        .RuleFor(x => x.ItemFlags, x => x.PickRandom<ItemFlags>());
        //    faker.Generate( x => x.ItemId = 5 );
        //}

        // Armor
        [TestMethod]
        public void ItemArmor_To_ItemData_Test()
        {
            ItemArmorBlueprint blueprint = new ItemArmorBlueprint { Id = 1, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Bash = 150, ItemFlags = ItemFlags.Glowing };
            IItemArmor armor = new ItemArmor(Guid.NewGuid(), blueprint, new Mock<IRoom>().Object);

            ItemData itemData = armor.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(armor.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(armor.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(armor.BaseItemFlags, itemData.ItemFlags);
        }

        // Boat
        [TestMethod]
        public void ItemBoat_To_ItemData_Test()
        {
            ItemBoatBlueprint blueprint = new ItemBoatBlueprint { Id = 1, Name = "Boat", ShortDescription = "BoatShort", Description = "BoatDesc", ItemFlags = ItemFlags.Glowing };
            IItemBoat boat = new ItemBoat(Guid.NewGuid(), blueprint, new Mock<IRoom>().Object);

            ItemData itemData = boat.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(boat.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(boat.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(boat.BaseItemFlags, itemData.ItemFlags);
        }

        // Staff
        [TestMethod]
        public void ItemStaff_To_ItemData_Test()
        {
            ItemStaffBlueprint blueprint = new ItemStaffBlueprint
            {
                Id = 1, Name = "Staff", ShortDescription = "StaffShort", Description = "StaffDesc", ItemFlags = ItemFlags.AntiEvil,
                MaxChargeCount = 10,
                CurrentChargeCount = 7,
                AlreadyRecharged = true
            };
            IItemStaff staff = new ItemStaff(Guid.NewGuid(), blueprint, new Mock<IContainer>().Object);

            ItemData itemData = staff.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemStaffData));
            Assert.AreEqual(staff.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(staff.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(staff.BaseItemFlags, itemData.ItemFlags);
            Assert.AreEqual(staff.MaxChargeCount, (itemData as ItemStaffData).MaxChargeCount);
            Assert.AreEqual(staff.CurrentChargeCount, (itemData as ItemStaffData).CurrentChargeCount);
            Assert.AreEqual(staff.AlreadyRecharged, (itemData as ItemStaffData).AlreadyRecharged);
        }

        // Wand
        [TestMethod]
        public void ItemWand_To_ItemData_Test()
        {
            ItemWandBlueprint blueprint = new ItemWandBlueprint
            {
                Id = 1, Name = "Wand", ShortDescription = "WandShort", Description = "WandDesc", ItemFlags = ItemFlags.AntiEvil,
                MaxChargeCount = 10,
                CurrentChargeCount = 7,
                AlreadyRecharged = true
            };
            IItemWand wand = new ItemWand(Guid.NewGuid(), blueprint, new Mock<IContainer>().Object);

            ItemData itemData = wand.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemWandData));
            Assert.AreEqual(wand.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(wand.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(wand.BaseItemFlags, itemData.ItemFlags);
            Assert.AreEqual(wand.MaxChargeCount, (itemData as ItemWandData).MaxChargeCount);
            Assert.AreEqual(wand.CurrentChargeCount, (itemData as ItemWandData).CurrentChargeCount);
            Assert.AreEqual(wand.AlreadyRecharged, (itemData as ItemWandData).AlreadyRecharged);
        }

        // WarpStone
        [TestMethod]
        public void ItemWarpstone_To_ItemData_Test()
        {
            ItemWarpStoneBlueprint blueprint = new ItemWarpStoneBlueprint { Id = 1, Name = "Warp", ShortDescription = "WarpShort", Description = "WarpDesc", ItemFlags = ItemFlags.AntiEvil };
            IItemWarpstone warpstone = new ItemWarpstone(Guid.NewGuid(), blueprint, new Mock<IContainer>().Object);

            ItemData itemData = warpstone.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(warpstone.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(warpstone.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(warpstone.BaseItemFlags, itemData.ItemFlags);
        }

        // Food
        [TestMethod]
        public void ItemFood_To_ItemData_Test()
        {
            ItemFoodBlueprint blueprint = new ItemFoodBlueprint { Id = 1, Name = "Food", ShortDescription = "FoodShort", Description = "FoodDesc", Cost = 20, HungerHours = 10, FullHours = 20, IsPoisoned = true };
            IItemFood food = new ItemFood(Guid.NewGuid(), blueprint, new Mock<IRoom>().Object);

            ItemData itemData = food.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemFoodData));
            Assert.AreEqual(food.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(food.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(food.BaseItemFlags, itemData.ItemFlags);
            Assert.AreEqual(food.FullHours, (itemData as ItemFoodData).FullHours);
            Assert.AreEqual(food.HungerHours, (itemData as ItemFoodData).HungerHours);
            Assert.AreEqual(food.IsPoisoned, (itemData as ItemFoodData).IsPoisoned);
        }

        // DrinkContainer
        [TestMethod]
        public void ItemDrinkContainer_To_ItemData_Test()
        {
            ItemDrinkContainerBlueprint blueprint = new ItemDrinkContainerBlueprint { Id = 1, Name = "Drink", ShortDescription = "DrinkShort", Description = "DrinkDesc", Cost = 10, CurrentLiquidAmount = 100, MaxLiquidAmount = 350, LiquidType = "water" };
            IItemDrinkContainer drinkContainer = new ItemDrinkContainer(Guid.NewGuid(), blueprint, new Mock<IRoom>().Object);

            ItemData itemData = drinkContainer.MapItemData();

            Assert.IsNotNull(drinkContainer);
            Assert.IsInstanceOfType(itemData, typeof(ItemDrinkContainerData));
            Assert.AreEqual(drinkContainer.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(drinkContainer.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(drinkContainer.BaseItemFlags, itemData.ItemFlags);
            Assert.AreEqual(drinkContainer.LiquidLeft, (itemData as ItemDrinkContainerData).CurrentLiquidAmount);
            Assert.AreEqual(drinkContainer.MaxLiquid, (itemData as ItemDrinkContainerData).MaxLiquidAmount);
            Assert.AreEqual(drinkContainer.LiquidName, (itemData as ItemDrinkContainerData).LiquidName);
            Assert.AreEqual(drinkContainer.IsPoisoned, (itemData as ItemDrinkContainerData).IsPoisoned);
        }

        // Container
        [TestMethod]
        public void ItemContainer_Empty_To_ItemData_Test()
        {
            ItemContainerBlueprint blueprint = new ItemContainerBlueprint
            {
                Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc",
                MaxWeight = 100, WeightMultiplier = 50, ContainerFlags = ContainerFlags.NoLock | ContainerFlags.Closed
            };
            IItemContainer container = new ItemContainer(Guid.NewGuid(), blueprint, new Mock<IRoom>().Object);

            ItemData itemData = container.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemContainerData));
            Assert.AreEqual(container.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(container.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(container.BaseItemFlags, itemData.ItemFlags);
            Assert.IsNull((itemData as ItemContainerData).Contains);
            Assert.AreEqual(container.ContainerFlags, (itemData as ItemContainerData).ContainerFlags);
            Assert.AreEqual(container.MaxWeight, (itemData as ItemContainerData).MaxWeight);
            Assert.AreEqual(container.MaxWeightPerItem, (itemData as ItemContainerData).MaxWeightPerItem);
        }

        [TestMethod]
        public void ItemContainer_OneItem_To_ItemData_Test()
        {
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", MaxWeight = 100, WeightMultiplier = 50 };
            IItemContainer container = new ItemContainer(Guid.NewGuid(), containerBlueprint, new Mock<IContainer>().Object);
            IItemLight light = new ItemLight(Guid.NewGuid(), new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 }, container);

            ItemData itemData = container.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemContainerData));
            Assert.AreEqual(container.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(container.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(container.BaseItemFlags, itemData.ItemFlags);
            Assert.IsNotNull((itemData as ItemContainerData).Contains);
            Assert.AreEqual(1, (itemData as ItemContainerData).Contains.Length);
            Assert.AreEqual(1, (itemData as ItemContainerData).Contains[0].ItemId);
            Assert.AreEqual(light.DecayPulseLeft, (itemData as ItemContainerData).Contains[0].DecayPulseLeft);
        }

        [TestMethod] // World is needed because when World.AddItem is used when serializing content
        public void ItemContainer_MultipleItems_To_ItemData_Test()
        {
            IItemContainer container = new ItemContainer(Guid.NewGuid(), new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", MaxWeight = 100, WeightMultiplier = 50 }, new Mock<IContainer>().Object);
            IItemLight light = new ItemLight(Guid.NewGuid(), new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 }, container);
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), new ItemPortalBlueprint { Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = -1 }, new Mock<IRoom>().Object, container);

            ItemData itemData = container.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemContainerData));
            Assert.AreEqual(container.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(container.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(container.BaseItemFlags, itemData.ItemFlags);
            Assert.IsNotNull((itemData as ItemContainerData).Contains);
            Assert.AreEqual(2, (itemData as ItemContainerData).Contains.Length);
            Assert.AreEqual(1, (itemData as ItemContainerData).Contains.Count(x => x.ItemId == 1));
            Assert.AreEqual(1, (itemData as ItemContainerData).Contains.Count(x => x.ItemId == 2));
            Assert.AreEqual(light.DecayPulseLeft + portal.DecayPulseLeft, (itemData as ItemContainerData).Contains.Sum(x => x.DecayPulseLeft));
        }

        [TestMethod]
        public void ItemContainer_NestedItems_To_ItemData_Test()
        {
            IItemContainer container1 = new ItemContainer(Guid.NewGuid(), new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", MaxWeight = 100, WeightMultiplier = 50 }, new Mock<IContainer>().Object);
            IItemLight light = new ItemLight(Guid.NewGuid(), new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 }, container1);
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), new ItemPortalBlueprint { Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 1 }, new Mock<IRoom>().Object, container1);
            IItemContainer container2 = new ItemContainer(Guid.NewGuid(), new ItemContainerBlueprint { Id = 888, Name = "Container2", ShortDescription = "Container2Short", Description = "Container2Desc", MaxWeight = 100, WeightMultiplier = 50 }, container1);
            IItemJewelry jewelry = new ItemJewelry(Guid.NewGuid(), new ItemJewelryBlueprint { Id = 3, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc" }, container2);
            IItemArmor armor = new ItemArmor(Guid.NewGuid(), new ItemArmorBlueprint { Id = 4, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Bash = 150 }, container2);

            ItemData itemData = container1.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemContainerData));
            Assert.AreEqual(container1.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(container1.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(container1.BaseItemFlags, itemData.ItemFlags);
            Assert.IsNotNull((itemData as ItemContainerData).Contains);
            Assert.AreEqual(3, (itemData as ItemContainerData).Contains.Length);
            Assert.AreEqual(1, (itemData as ItemContainerData).Contains.Count(x => x.ItemId == 1));
            Assert.AreEqual(1, (itemData as ItemContainerData).Contains.Count(x => x.ItemId == 2));
            Assert.AreEqual(1, (itemData as ItemContainerData).Contains.Count(x => x.ItemId == 888));
            ItemData nestedContainer = (itemData as ItemContainerData).Contains.Single(x => x.ItemId == 888);
            Assert.IsInstanceOfType(nestedContainer, typeof(ItemContainerData));
            Assert.IsNotNull((nestedContainer as ItemContainerData).Contains);
            Assert.AreEqual(2, (nestedContainer as ItemContainerData).Contains.Length);
            Assert.AreEqual(1, (nestedContainer as ItemContainerData).Contains.Count(x => x.ItemId == 3));
            Assert.AreEqual(1, (nestedContainer as ItemContainerData).Contains.Count(x => x.ItemId == 4));
        }

        // Corpse
        [TestMethod]
        public void NPCItemCorpse_Empty_To_ItemData_Test()
        {
            INonPlayableCharacter character = new Character.NonPlayableCharacter.NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1", ShortDescription = "Mob1Short", Description = "Mob1Desc", Level = 1, Sex = Sex.Male }, new Mock<IRoom>().Object);
            IItemCorpse corpse = new ItemCorpse(Guid.NewGuid(), new ItemCorpseBlueprint { Id = 999, Name = "Corpse" }, new Mock<IRoom>().Object, character);

            ItemData itemData = corpse.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemCorpseData));
            Assert.IsFalse((itemData as ItemCorpseData).IsPlayableCharacterCorpse);
            Assert.AreEqual(corpse.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(corpse.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(corpse.BaseItemFlags, itemData.ItemFlags);
            Assert.AreEqual(character.DisplayName, (itemData as ItemCorpseData).CorpseName);
            Assert.IsNull((itemData as ItemCorpseData).Contains);
        }

        [TestMethod]
        public void PCItemCorpse_Empty_To_ItemData_Test()
        {
            IPlayableCharacter character = new Character.PlayableCharacter.PlayableCharacter(Guid.NewGuid(), new CharacterData { Name = "Impersonate1", Level = 1, Sex = Sex.Male, Class = "Mage", Race = "Human", RoomId = 1}, new Player.Player(Guid.NewGuid(), "Player1"), new Mock<IRoom>().Object);
            IItemCorpse corpse = new ItemCorpse(Guid.NewGuid(), new ItemCorpseBlueprint { Id = 999, Name = "Corpse" }, new Mock<IRoom>().Object, character);

            ItemData itemData = corpse.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemCorpseData));
            Assert.IsTrue((itemData as ItemCorpseData).IsPlayableCharacterCorpse);
            Assert.AreEqual(corpse.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(corpse.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(corpse.BaseItemFlags, itemData.ItemFlags);
            Assert.AreEqual(character.DisplayName, (itemData as ItemCorpseData).CorpseName);
            Assert.IsNull((itemData as ItemCorpseData).Contains);
        }

        [TestMethod]
        public void NPCItemCorpse_OneItem_To_ItemData_Test()
        {
            INonPlayableCharacter character = new Character.NonPlayableCharacter.NonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1", ShortDescription = "Mob1Short", Description = "Mob1Desc", Level = 1, Sex = Sex.Male }, new Mock<IRoom>().Object);
            IItemLight light = new ItemLight(Guid.NewGuid(), new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 }, character);
            IItemCorpse corpse = new ItemCorpse(Guid.NewGuid(), new ItemCorpseBlueprint { Id = 999, Name = "Corpse" }, new Mock<IRoom>().Object, character);

            ItemData itemData = corpse.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemCorpseData));
            Assert.IsFalse((itemData as ItemCorpseData).IsPlayableCharacterCorpse);
            Assert.AreEqual(corpse.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(corpse.BaseItemFlags, itemData.ItemFlags);
            Assert.AreEqual(character.DisplayName, (itemData as ItemCorpseData).CorpseName);
            Assert.IsNotNull((itemData as ItemCorpseData).Contains);
            Assert.AreEqual(1, (itemData as ItemCorpseData).Contains.Length);
            Assert.AreEqual(light.Blueprint.Id, (itemData as ItemCorpseData).Contains[0].ItemId);
        }

        // Furniture
        [TestMethod]
        public void ItemFurniture_To_ItemData_Test()
        {
            IItemFurniture furniture = new ItemFurniture(Guid.NewGuid(), new ItemFurnitureBlueprint { Id = 1, Name = "Furniture", ShortDescription = "FurnitureShort", Description = "FurnitureDesc", NoTake = true, FurnitureActions = FurnitureActions.Sleep, FurniturePlacePreposition = FurniturePlacePrepositions.On, MaxPeople = 10 }, new Mock<IContainer>().Object);

            ItemData itemData = furniture.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(furniture.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(furniture.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(furniture.BaseItemFlags, itemData.ItemFlags);
        }

        // Jewelry
        [TestMethod]
        public void ItemJewelry_To_ItemData_Test()
        {
            IItemJewelry jewelry = new ItemJewelry(Guid.NewGuid(), new ItemJewelryBlueprint { Id = 1, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc", ItemFlags = ItemFlags.Glowing}, new Mock<IContainer>().Object);

            ItemData itemData = jewelry.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(jewelry.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(jewelry.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(jewelry.BaseItemFlags, itemData.ItemFlags);
        }

        // Key
        [TestMethod]
        public void ItemKey_To_ItemData_Test()
        {
            IItemKey key = new ItemKey(Guid.NewGuid(), new ItemKeyBlueprint { Id = 1, Name = "Key", ShortDescription = "KeyShort", Description = "KeyDesc" }, new Mock<IContainer>().Object);

            ItemData itemData = key.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(key.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(key.DecayPulseLeft, itemData.DecayPulseLeft);
        }

        // Light
        [TestMethod]
        public void ItemLight_To_ItemData_Test()
        {
            IItemLight light = new ItemLight(Guid.NewGuid(), new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", ItemFlags = ItemFlags.Glowing, DurationHours = 5 }, new Mock<IContainer>().Object);

            ItemData itemData = light.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemLightData));
            Assert.AreEqual(light.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(light.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(light.BaseItemFlags, itemData.ItemFlags);
            Assert.AreEqual(light.TimeLeft, (itemData as ItemLightData).TimeLeft);
        }

        // Portal
        [TestMethod]
        public void ItemPortal_To_ItemData_Test()
        {
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), new ItemPortalBlueprint { Id = 1, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = 1, MaxChargeCount = 10, CurrentChargeCount = 5, PortalFlags = PortalFlags.GoWith | PortalFlags.NoLock }, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            ItemData itemData = portal.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemPortalData));
            Assert.AreEqual(portal.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(portal.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(portal.MaxChargeCount, (itemData as ItemPortalData).MaxChargeCount);
            Assert.AreEqual(portal.CurrentChargeCount, (itemData as ItemPortalData).CurrentChargeCount);
            Assert.AreEqual(portal.PortalFlags, (itemData as ItemPortalData).PortalFlags);
        }

        // Quest
        [TestMethod]
        public void ItemQuest_To_ItemData_Test()
        {
            IItemQuest quest = new ItemQuest(Guid.NewGuid(), new ItemQuestBlueprint { Id = 1, Name = "Quest", ShortDescription = "QuestShort", Description = "QuestDesc" }, new Mock<IContainer>().Object);

            ItemData itemData = quest.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(quest.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(quest.DecayPulseLeft, itemData.DecayPulseLeft);
        }

        // Shield
        [TestMethod]
        public void ItemShield_To_ItemData_Test()
        {
            IItemShield shield = new ItemShield(Guid.NewGuid(), new ItemShieldBlueprint { Id = 1, Name = "Shield", ShortDescription = "ShieldShort", Description = "ShieldDesc", Armor = 150 }, new Mock<IContainer>().Object);

            ItemData itemData = shield.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(shield.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(shield.DecayPulseLeft, itemData.DecayPulseLeft);
        }

        // Weapon
        [TestMethod]
        public void ItemWeapon_To_ItemData_Test()
        {
            IItemWeapon weapon = new ItemWeapon(Guid.NewGuid(), new ItemWeaponBlueprint { Id = 1, Name = "Weapon", ShortDescription = "WeaponShort", Description = "WeaponDesc", ItemFlags = ItemFlags.NoDrop, DamageType = SchoolTypes.Fire, DiceCount = 10, DiceValue = 20, Flags = WeaponFlags.Shocking | WeaponFlags.Vampiric }, new Mock<IContainer>().Object);

            ItemData itemData = weapon.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemWeaponData));
            Assert.AreEqual(weapon.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(weapon.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(weapon.BaseItemFlags, itemData.ItemFlags);
            Assert.AreEqual(weapon.BaseWeaponFlags, (itemData as ItemWeaponData).WeaponFlags);
        }
    }
}
