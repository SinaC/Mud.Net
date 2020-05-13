using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Item;

namespace Mud.Server.Tests
{
    [TestClass]
    public class ItemTests
    {
        // Wand/Staff
        [TestMethod]
        public void Staff_Creation_Values()
        {
            ItemStaffBlueprint blueprint = new ItemStaffBlueprint
            {
                Id = 1, Name = "Staff", ShortDescription = "StaffShort", Description = "StaffDesc", ItemFlags = ItemFlags.AntiEvil,
                MaxChargeCount = 10,
                CurrentChargeCount = 7,
                AlreadyRecharged = false
            };

            IItemStaff staff = new ItemStaff(Guid.NewGuid(), blueprint, new Mock<IContainer>().Object);

            Assert.AreEqual(blueprint.MaxChargeCount, staff.MaxChargeCount);
            Assert.AreEqual(blueprint.CurrentChargeCount, staff.CurrentChargeCount);
            Assert.AreEqual(blueprint.AlreadyRecharged, staff.AlreadyRecharged);
        }

        [TestMethod]
        public void Staff_Use()
        {
            ItemStaffBlueprint blueprint = new ItemStaffBlueprint
            {
                Id = 1, Name = "Staff", ShortDescription = "StaffShort", Description = "StaffDesc", ItemFlags = ItemFlags.AntiEvil,
                MaxChargeCount = 10,
                CurrentChargeCount = 7,
                AlreadyRecharged = false
            };
            IItemStaff staff = new ItemStaff(Guid.NewGuid(), blueprint, new Mock<IContainer>().Object);

            staff.Use();

            Assert.AreEqual(10, staff.MaxChargeCount);
            Assert.AreEqual(6, staff.CurrentChargeCount);
            Assert.IsFalse(staff.AlreadyRecharged);
        }

        [TestMethod]
        public void Staff_Recharge()
        {
            ItemStaffBlueprint blueprint = new ItemStaffBlueprint
            {
                Id = 1, Name = "Staff", ShortDescription = "StaffShort", Description = "StaffDesc", ItemFlags = ItemFlags.AntiEvil,
                MaxChargeCount = 10,
                CurrentChargeCount = 7,
                AlreadyRecharged = false
            };
            IItemStaff staff = new ItemStaff(Guid.NewGuid(), blueprint, new Mock<IContainer>().Object);

            staff.Recharge(7, 9);

            Assert.AreEqual(9, staff.MaxChargeCount);
            Assert.AreEqual(7, staff.CurrentChargeCount);
            Assert.IsTrue(staff.AlreadyRecharged);
        }

        // Portal
        [TestMethod]
        public void Portal_Creation_Values()
        {
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1, Name = "portal", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
                MaxChargeCount = 10,
                CurrentChargeCount = 7,
            };
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), portalBlueprint, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Use()
        {
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1, Name = "portal", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
                MaxChargeCount = 10,
                CurrentChargeCount = 7,
            };
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), portalBlueprint, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            portal.Use();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount-1, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Use_InfiniteCharges()
        {
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1, Name = "portal", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
                MaxChargeCount = -1,
                CurrentChargeCount = 7,
            };
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), portalBlueprint, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            portal.Use();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Open_NoClosed()
        {
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1, Name = "portal", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                PortalFlags = PortalFlags.PickProof,
            };
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), portalBlueprint, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            portal.Open();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Open() 
        {
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1, Name = "portal", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
            };
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), portalBlueprint, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            portal.Open();

            Assert.AreEqual(portalBlueprint.PortalFlags & ~PortalFlags.Closed, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Close_NoCloseable()
        {
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1, Name = "portal", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                PortalFlags = PortalFlags.PickProof | PortalFlags.NoClose,
            };
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), portalBlueprint, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            portal.Close();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Close() 
        {
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1, Name = "portal", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                PortalFlags = PortalFlags.PickProof,
            };
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), portalBlueprint, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            portal.Close();

            Assert.AreEqual(portalBlueprint.PortalFlags | PortalFlags.Closed, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Lock_NoLockable()
        {
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1, Name = "portal", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.NoLock | PortalFlags.PickProof,
            };
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), portalBlueprint, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            portal.Lock();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Lock()
        {
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1, Name = "portal", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
            };
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), portalBlueprint, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            portal.Lock();

            Assert.AreEqual(portalBlueprint.PortalFlags | PortalFlags.Locked, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Unlock_NotLocked()
        {
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1, Name = "portal", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof,
            };
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), portalBlueprint, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            portal.Unlock();

            Assert.AreEqual(portalBlueprint.PortalFlags, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        [TestMethod]
        public void Portal_Unlock()
        {
            ItemPortalBlueprint portalBlueprint = new ItemPortalBlueprint
            {
                Id = 1, Name = "portal", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                PortalFlags = PortalFlags.Closed | PortalFlags.PickProof | PortalFlags.Locked,
            };
            IItemPortal portal = new ItemPortal(Guid.NewGuid(), portalBlueprint, new Mock<IRoom>().Object, new Mock<IContainer>().Object);

            portal.Unlock();

            Assert.AreEqual(portalBlueprint.PortalFlags & ~PortalFlags.Locked, portal.PortalFlags);
            Assert.AreEqual(portalBlueprint.MaxChargeCount, portal.MaxChargeCount);
            Assert.AreEqual(portalBlueprint.CurrentChargeCount, portal.CurrentChargeCount);
        }

        // Container
        [TestMethod]
        public void Container_Open_NoClosed()
        {
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1, Name = "container", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof,
            };
            IItemContainer container = new ItemContainer(Guid.NewGuid(), containerBlueprint, new Mock<IContainer>().Object);

            container.Open();

            Assert.AreEqual(containerBlueprint.ContainerFlags, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Open()
        {
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1, Name = "container", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.Closed,
            };
            IItemContainer container = new ItemContainer(Guid.NewGuid(), containerBlueprint, new Mock<IContainer>().Object);

            container.Open();

            Assert.AreEqual(containerBlueprint.ContainerFlags & ~ContainerFlags.Closed, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Close_NoCloseable()
        {
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1, Name = "container", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.NoClose,
            };
            IItemContainer container = new ItemContainer(Guid.NewGuid(), containerBlueprint, new Mock<IContainer>().Object);

            container.Close();

            Assert.AreEqual(containerBlueprint.ContainerFlags, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Close()
        {
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1, Name = "container", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof,
            };
            IItemContainer container = new ItemContainer(Guid.NewGuid(), containerBlueprint, new Mock<IContainer>().Object);

            container.Close();

            Assert.AreEqual(containerBlueprint.ContainerFlags | ContainerFlags.Closed, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Lock_NoLockable()
        {
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1, Name = "container", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.Closed | ContainerFlags.NoLock,
            };
            IItemContainer container = new ItemContainer(Guid.NewGuid(), containerBlueprint, new Mock<IContainer>().Object);

            container.Lock();

            Assert.AreEqual(containerBlueprint.ContainerFlags, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Lock()
        {
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1, Name = "container", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.Closed,
            };
            IItemContainer container = new ItemContainer(Guid.NewGuid(), containerBlueprint, new Mock<IContainer>().Object);

            container.Lock();

            Assert.AreEqual(containerBlueprint.ContainerFlags | ContainerFlags.Locked, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Unlock_NotLocked()
        {
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1, Name = "container", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.Closed,
            };
            IItemContainer container = new ItemContainer(Guid.NewGuid(), containerBlueprint, new Mock<IContainer>().Object);

            container.Unlock();

            Assert.AreEqual(containerBlueprint.ContainerFlags, container.ContainerFlags);
        }

        [TestMethod]
        public void Container_Unlock()
        {
            ItemContainerBlueprint containerBlueprint = new ItemContainerBlueprint
            {
                Id = 1, Name = "container", ShortDescription = "PortalShort", Description = "PortalDesc",
                Key = 10,
                ContainerFlags = ContainerFlags.PickProof | ContainerFlags.Closed | ContainerFlags.Locked,
            };
            IItemContainer container = new ItemContainer(Guid.NewGuid(), containerBlueprint, new Mock<IContainer>().Object);

            container.Unlock();

            Assert.AreEqual(containerBlueprint.ContainerFlags & ~ContainerFlags.Locked, container.ContainerFlags);
        }

        // Food
        [TestMethod]
        public void Food_Creation_Values()
        {
            ItemFoodBlueprint foodBlueprint = new ItemFoodBlueprint
            {
                Id = 1, Name = "Food", ShortDescription = "FoodShort", Description = "FoodDesc", Cost = 20,
                HungerHours = 10,
                FullHours = 20,
                IsPoisoned = true
            };
            IItemFood food = new ItemFood(Guid.NewGuid(), foodBlueprint, new Mock<IRoom>().Object);

            Assert.AreEqual(20, food.FullHours);
            Assert.AreEqual(10, food.HungerHours);
            Assert.IsTrue(food.IsPoisoned);
        }

        [TestMethod]
        public void Food_Poison() 
        {
            ItemFoodBlueprint foodBlueprint = new ItemFoodBlueprint 
            { 
                Id = 1, Name = "Food", ShortDescription = "FoodShort", Description = "FoodDesc", Cost = 20, 
                HungerHours = 10,
                FullHours = 20,
                IsPoisoned = false
            };
            IItemFood food = new ItemFood(Guid.NewGuid(), foodBlueprint, new Mock<IRoom>().Object);

            food.Poison();

            Assert.IsTrue(food.IsPoisoned);
        }

        [TestMethod]
        public void Food_Cure()
        {
            ItemFoodBlueprint foodBlueprint = new ItemFoodBlueprint
            {
                Id = 1, Name = "Food",ShortDescription = "FoodShort",  Description = "FoodDesc", Cost = 20,
                HungerHours = 10,
                FullHours = 20,
                IsPoisoned = true
            };
            IItemFood food = new ItemFood(Guid.NewGuid(), foodBlueprint, new Mock<IRoom>().Object);

            food.Cure();

            Assert.IsFalse(food.IsPoisoned);
        }

        [TestMethod]
        public void Food_SetHours()
        {
            ItemFoodBlueprint foodBlueprint = new ItemFoodBlueprint
            {
                Id = 1, Name = "Food", ShortDescription = "FoodShort", Description = "FoodDesc", Cost = 20,
                HungerHours = 10,
                FullHours = 20,
                IsPoisoned = true
            };
            IItemFood food = new ItemFood(Guid.NewGuid(), foodBlueprint, new Mock<IRoom>().Object);

            food.SetHours(10, 15);

            Assert.AreEqual(10, food.FullHours);
            Assert.AreEqual(15, food.HungerHours);
        }

        // Fountain
        [TestMethod]
        public void Fountain_Creation_Values()
        {
            ItemFountainBlueprint fountainBlueprint = new ItemFountainBlueprint
            {
                Id = 1, Name = "fountain", ShortDescription = "FountainShort", Description = "FountainDesc",
                LiquidType = "water"
            };
            IItemFountain fountain = new ItemFountain(Guid.NewGuid(), fountainBlueprint, new Mock<IRoom>().Object);

            Assert.AreEqual(fountainBlueprint.LiquidType, fountain.LiquidName);
            Assert.AreEqual(3, fountain.LiquidAmountMultiplier);
            Assert.AreEqual(int.MaxValue, fountain.LiquidLeft);
            Assert.IsFalse(fountain.IsEmpty);
        }

        [TestMethod]
        public void Fountain_Drink_Infinite()
        {
            ItemFountainBlueprint fountainBlueprint = new ItemFountainBlueprint
            {
                Id = 1, Name = "fountain", ShortDescription = "FountainShort", Description = "FountainDesc",
                LiquidType = "water"
            };
            IItemFountain fountain = new ItemFountain(Guid.NewGuid(), fountainBlueprint, new Mock<IRoom>().Object);

            fountain.Drink(10000000);

            Assert.AreEqual(fountainBlueprint.LiquidType, fountain.LiquidName);
            Assert.AreEqual(3, fountain.LiquidAmountMultiplier);
            Assert.AreEqual(int.MaxValue, fountain.LiquidLeft);
            Assert.IsFalse(fountain.IsEmpty);
        }

        // DrinkContainer
        [TestMethod]
        public void DrinkContainer_Creation_Values()
        {
            ItemDrinkContainerBlueprint drinkContainerBlueprint = new ItemDrinkContainerBlueprint
            {
                Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
                MaxLiquidAmount = 500,
                CurrentLiquidAmount = 350,
                LiquidType = "water",
                IsPoisoned = true
            };
            IItemDrinkContainer drinkContainer = new ItemDrinkContainer(Guid.NewGuid(), drinkContainerBlueprint, new Mock<IRoom>().Object);

            Assert.AreEqual(drinkContainerBlueprint.LiquidType, drinkContainer.LiquidName);
            Assert.AreEqual(1, drinkContainer.LiquidAmountMultiplier);
            Assert.AreEqual(drinkContainerBlueprint.MaxLiquidAmount, drinkContainer.MaxLiquid);
            Assert.AreEqual(drinkContainerBlueprint.CurrentLiquidAmount, drinkContainer.LiquidLeft);
            Assert.IsFalse(drinkContainer.IsEmpty);
            Assert.IsTrue(drinkContainer.IsPoisoned);
        }

        [TestMethod]
        public void DrinkContainer_Drink_LessThanLiquidLeft()
        {
            ItemDrinkContainerBlueprint drinkContainerBlueprint = new ItemDrinkContainerBlueprint
            {
                Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
                MaxLiquidAmount = 500,
                CurrentLiquidAmount = 350,
                LiquidType = "water",
                IsPoisoned = true
            };
            IItemDrinkContainer drinkContainer = new ItemDrinkContainer(Guid.NewGuid(), drinkContainerBlueprint, new Mock<IRoom>().Object);
            drinkContainer.Drink(100);

            Assert.IsFalse(drinkContainer.IsEmpty);
            Assert.AreEqual(250, drinkContainer.LiquidLeft);
            Assert.AreEqual(500, drinkContainer.MaxLiquid);
        }

        [TestMethod]
        public void DrinkContainer_Drink_MoreThanLiquidLeft()
        {
            ItemDrinkContainerBlueprint drinkContainerBlueprint = new ItemDrinkContainerBlueprint
            {
                Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
                MaxLiquidAmount = 500,
                CurrentLiquidAmount = 350,
                LiquidType = "water",
                IsPoisoned = true
            };
            IItemDrinkContainer drinkContainer = new ItemDrinkContainer(Guid.NewGuid(), drinkContainerBlueprint, new Mock<IRoom>().Object);
            drinkContainer.Drink(1000);

            Assert.IsTrue(drinkContainer.IsEmpty);
            Assert.AreEqual(0, drinkContainer.LiquidLeft);
            Assert.AreEqual(500, drinkContainer.MaxLiquid);
        }

        [TestMethod]
        public void DrinkContainer_Drink_Poison()
        {
            ItemDrinkContainerBlueprint drinkContainerBlueprint = new ItemDrinkContainerBlueprint
            {
                Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
                MaxLiquidAmount = 500,
                CurrentLiquidAmount = 350,
                LiquidType = "water",
                IsPoisoned = false
            };
            IItemDrinkContainer drinkContainer = new ItemDrinkContainer(Guid.NewGuid(), drinkContainerBlueprint, new Mock<IRoom>().Object);
            drinkContainer.Poison();

            Assert.IsTrue(drinkContainer.IsPoisoned);
            Assert.AreEqual(350, drinkContainer.LiquidLeft);
            Assert.AreEqual(500, drinkContainer.MaxLiquid);
        }

        [TestMethod]
        public void DrinkContainer_Drink_Cure()
        {
            ItemDrinkContainerBlueprint drinkContainerBlueprint = new ItemDrinkContainerBlueprint
            {
                Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
                MaxLiquidAmount = 500,
                CurrentLiquidAmount = 350,
                LiquidType = "water",
                IsPoisoned = true
            };
            IItemDrinkContainer drinkContainer = new ItemDrinkContainer(Guid.NewGuid(), drinkContainerBlueprint, new Mock<IRoom>().Object);
            drinkContainer.Cure();

            Assert.IsFalse(drinkContainer.IsPoisoned);
            Assert.AreEqual(350, drinkContainer.LiquidLeft);
            Assert.AreEqual(500, drinkContainer.MaxLiquid);
        }

        [TestMethod]
        public void DrinkContainer_Drink_Filld()
        {
            ItemDrinkContainerBlueprint drinkContainerBlueprint = new ItemDrinkContainerBlueprint
            {
                Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
                MaxLiquidAmount = 500,
                CurrentLiquidAmount = 350,
                LiquidType = "water",
                IsPoisoned = true
            };
            IItemDrinkContainer drinkContainer = new ItemDrinkContainer(Guid.NewGuid(), drinkContainerBlueprint, new Mock<IRoom>().Object);
            drinkContainer.Fill(500);

            Assert.IsTrue(drinkContainer.IsPoisoned);
            Assert.AreEqual("water", drinkContainer.LiquidName);
            Assert.AreEqual(500, drinkContainer.LiquidLeft);
            Assert.AreEqual(500, drinkContainer.MaxLiquid);
        }

        [TestMethod]
        public void DrinkContainer_Drink_Fill_ChangeLiquid()
        {
            ItemDrinkContainerBlueprint drinkContainerBlueprint = new ItemDrinkContainerBlueprint
            {
                Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
                MaxLiquidAmount = 500,
                CurrentLiquidAmount = 350,
                LiquidType = "water",
                IsPoisoned = true
            };
            IItemDrinkContainer drinkContainer = new ItemDrinkContainer(Guid.NewGuid(), drinkContainerBlueprint, new Mock<IRoom>().Object);
            drinkContainer.Fill("wine", 500);

            Assert.IsTrue(drinkContainer.IsPoisoned);
            Assert.AreEqual("wine", drinkContainer.LiquidName);
            Assert.AreEqual(500, drinkContainer.LiquidLeft);
            Assert.AreEqual(500, drinkContainer.MaxLiquid);
        }

        [TestMethod]
        public void DrinkContainer_Drink_Pour()
        {
            ItemDrinkContainerBlueprint drinkContainerBlueprint = new ItemDrinkContainerBlueprint
            {
                Id = 1, Name = "drinkcontainer", ShortDescription = "DrinkContainerShort", Description = "DrinkContainerDesc",
                MaxLiquidAmount = 500,
                CurrentLiquidAmount = 350,
                LiquidType = "water",
                IsPoisoned = true
            };
            IItemDrinkContainer drinkContainer = new ItemDrinkContainer(Guid.NewGuid(), drinkContainerBlueprint, new Mock<IRoom>().Object);
            drinkContainer.Pour();

            Assert.IsFalse(drinkContainer.IsPoisoned);
            Assert.AreEqual(0, drinkContainer.LiquidLeft);
            Assert.AreEqual(500, drinkContainer.MaxLiquid);
        }
    }
}
