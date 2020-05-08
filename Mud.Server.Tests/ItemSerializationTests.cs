using System;
using System.Linq;
using AutoBogus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Blueprints.Room;
using Mud.Server.Item;

namespace Mud.Server.Tests
{
    [TestClass]
    public class ItemSerializationTests : TestBase
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
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemArmor armor = World.AddItem(Guid.NewGuid(), new ItemArmorBlueprint { Id = 1, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Bash = 150, ItemFlags = ItemFlags.Glowing }, room) as IItemArmor;

            ItemData itemData = armor.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(armor.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(armor.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(armor.BaseItemFlags, itemData.ItemFlags);
        }

        [TestMethod]
        public void ItemDrinkContainer_To_ItemData_Test()
        {
            IWorld world = new World.World();
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));

            IItemDrinkContainer drinkContainer = World.AddItem(Guid.NewGuid(), new ItemDrinkContainerBlueprint { Id = 1, Name = "Drink", ShortDescription = "DrinkShort", Description = "DrinkDesc", Cost = 10, CurrentLiquidAmount = 100, MaxLiquidAmount = 350, LiquidType = "water"}, room) as IItemDrinkContainer;

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
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemContainer container = world.AddItem(Guid.NewGuid(), new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", ItemCount = 10, WeightMultiplier = 50 }, room) as IItemContainer;

            ItemData itemData = container.MapItemData();

            Assert.IsInstanceOfType(itemData, typeof(ItemContainerData));
            Assert.AreEqual(container.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(container.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(container.BaseItemFlags, itemData.ItemFlags);
            Assert.IsNull((itemData as ItemContainerData).Contains);
        }

        [TestMethod]
        public void ItemContainer_OneItem_To_ItemData_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemContainer container = world.AddItem(Guid.NewGuid(), new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", ItemCount = 10, WeightMultiplier = 50 }, room) as IItemContainer;
            IItemLight light = world.AddItem(Guid.NewGuid(), new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 }, container) as IItemLight;

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

        [TestMethod]
        public void ItemContainer_MultipleItems_To_ItemData_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemContainer container = world.AddItem(Guid.NewGuid(), new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", ItemCount = 10, WeightMultiplier = 50 }, room) as IItemContainer;
            IItemLight light = world.AddItem(Guid.NewGuid(), new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 }, container) as IItemLight;
            IItemPortal portal = world.AddItem(Guid.NewGuid(), new ItemPortalBlueprint { Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = room.Blueprint.Id }, container) as IItemPortal;

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
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemContainer container1 = world.AddItem(Guid.NewGuid(), new ItemContainerBlueprint { Id = 999, Name = "Container", ShortDescription = "ContainerShort", Description = "ContainerDesc", ItemCount = 10, WeightMultiplier = 50 }, room) as IItemContainer;
            IItemLight light = world.AddItem(Guid.NewGuid(), new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 }, container1) as IItemLight;
            IItemPortal portal = world.AddItem(Guid.NewGuid(), new ItemPortalBlueprint { Id = 2, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = room.Blueprint.Id }, container1) as IItemPortal;
            IItemContainer container2 = world.AddItem(Guid.NewGuid(), new ItemContainerBlueprint { Id = 888, Name = "Container2", ShortDescription = "Container2Short", Description = "Container2Desc", ItemCount = 10, WeightMultiplier = 50 }, container1) as IItemContainer;
            IItemJewelry jewelry = world.AddItem(Guid.NewGuid(), new ItemJewelryBlueprint { Id = 3, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc" }, container2) as IItemJewelry;
            IItemArmor armor = world.AddItem(Guid.NewGuid(), new ItemArmorBlueprint { Id = 4, Name = "Armor", ShortDescription = "ArmorShort", Description = "ArmorDesc", Bash = 150 }, container2) as IItemArmor;

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
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint {Id = 1, Name = "room1"}, new Area.Area("Area", 1, 100, "builders", "credits"));
            INonPlayableCharacter character = world.AddNonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1", ShortDescription = "Mob1Short", Description = "Mob1Desc", Level = 1, Sex = Domain.Sex.Male }, room);
            IItemCorpse corpse = world.AddItemCorpse(Guid.NewGuid(), new ItemCorpseBlueprint { Id = 999, Name = "Corpse" }, room, character);

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
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IPlayableCharacter character = world.AddPlayableCharacter(Guid.NewGuid(), new CharacterData { Name = "Impersonate1", Level = 1, Sex = Sex.Male, Class = "Mage", Race = "Human", RoomId = 1}, new Player.Player(Guid.NewGuid(), "Player1"), room);
            IItemCorpse corpse = world.AddItemCorpse(Guid.NewGuid(), new ItemCorpseBlueprint { Id = 999, Name = "Corpse" }, room, character);

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
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            INonPlayableCharacter character = world.AddNonPlayableCharacter(Guid.NewGuid(), new Blueprints.Character.CharacterNormalBlueprint { Id = 1, Name = "Mob1", ShortDescription = "Mob1Short", Description = "Mob1Desc", Level = 1, Sex = Domain.Sex.Male }, room);
            IItemLight light = world.AddItem(Guid.NewGuid(), new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", DurationHours = 5 }, character) as IItemLight;
            IItemCorpse corpse = world.AddItemCorpse(Guid.NewGuid(), new ItemCorpseBlueprint { Id = 999, Name = "Corpse" }, room, character);

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
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemFurniture furniture = world.AddItem(Guid.NewGuid(), new ItemFurnitureBlueprint { Id = 1, Name = "Furniture", ShortDescription = "FurnitureShort", Description = "FurnitureDesc", NoTake = true, FurnitureActions = FurnitureActions.Sleep, FurniturePlacePreposition = FurniturePlacePrepositions.On, MaxPeople = 10 }, room) as IItemFurniture;

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
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemJewelry jewelry = world.AddItem(Guid.NewGuid(), new ItemJewelryBlueprint { Id = 1, Name = "Jewelry", ShortDescription = "JewelryShort", Description = "JewelryDesc", ItemFlags = ItemFlags.Glowing}, room) as IItemJewelry;

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
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemKey key = world.AddItem(Guid.NewGuid(), new ItemKeyBlueprint { Id = 1, Name = "Key", ShortDescription = "KeyShort", Description = "KeyDesc" }, room) as IItemKey;

            ItemData itemData = key.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(key.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(key.DecayPulseLeft, itemData.DecayPulseLeft);
        }

        // Light
        [TestMethod]
        public void ItemLight_To_ItemData_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemLight light = world.AddItem(Guid.NewGuid(), new ItemLightBlueprint { Id = 1, Name = "Light", ShortDescription = "LightShort", Description = "LightDesc", ItemFlags = ItemFlags.Glowing, DurationHours = 5 }, room) as IItemLight;

            ItemData itemData = light.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(light.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(light.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(light.BaseItemFlags, itemData.ItemFlags);
        }

        // Portal
        [TestMethod]
        public void ItemPortal_To_ItemData_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemPortal portal = world.AddItem(Guid.NewGuid(), new ItemPortalBlueprint { Id = 1, Name = "Portal", ShortDescription = "PortalShort", Description = "PortalDesc", Destination = room.Blueprint.Id }, room) as IItemPortal;

            ItemData itemData = portal.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(portal.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(portal.DecayPulseLeft, itemData.DecayPulseLeft);
        }

        // Quest
        [TestMethod]
        public void ItemQuest_To_ItemData_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemQuest quest = world.AddItem(Guid.NewGuid(), new ItemQuestBlueprint { Id = 1, Name = "Quest", ShortDescription = "QuestShort", Description = "QuestDesc" }, room) as IItemQuest;

            ItemData itemData = quest.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(quest.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(quest.DecayPulseLeft, itemData.DecayPulseLeft);
        }

        // Shield
        [TestMethod]
        public void ItemShield_To_ItemData_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemShield shield = world.AddItem(Guid.NewGuid(), new ItemShieldBlueprint { Id = 1, Name = "Shield", ShortDescription = "ShieldShort", Description = "ShieldDesc", Armor = 150 }, room) as IItemShield;

            ItemData itemData = shield.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(shield.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(shield.DecayPulseLeft, itemData.DecayPulseLeft);
        }

        // Weapon
        [TestMethod]
        public void ItemWeapon_To_ItemData_Test()
        {
            IWorld world = World;
            IRoom room = world.AddRoom(Guid.NewGuid(), new RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area", 1, 100, "builders", "credits"));
            IItemWeapon weapon = world.AddItem(Guid.NewGuid(), new ItemWeaponBlueprint { Id = 1, Name = "Weapon", ShortDescription = "WeaponShort", Description = "WeaponDesc", ItemFlags = ItemFlags.NoDrop, DamageType = SchoolTypes.Fire, DiceCount = 10, DiceValue = 20 }, room) as IItemWeapon;

            ItemData itemData = weapon.MapItemData(); // no specific ItemData

            Assert.IsInstanceOfType(itemData, typeof(ItemData));
            Assert.AreEqual(weapon.Blueprint.Id, itemData.ItemId);
            Assert.AreEqual(weapon.DecayPulseLeft, itemData.DecayPulseLeft);
            Assert.AreEqual(weapon.BaseItemFlags, itemData.ItemFlags);
        }
    }
}
