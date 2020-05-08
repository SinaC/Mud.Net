﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Mud.Server.Blueprints.Item;
using Mud.Server.Item;

namespace Mud.Server.Tests
{
    [TestClass]
    public class ItemTests
    {
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
            Assert.IsFalse(fountain.IsPoisoned);
        }

        [TestMethod]
        public void Fountain_Drink_Infinite()
        {
            ItemFountainBlueprint fountainBlueprint = new ItemFountainBlueprint
            {
                Id = 1,
                Name = "fountain",
                ShortDescription = "FountainShort",
                Description = "FountainDesc",
                LiquidType = "water"
            };
            IItemFountain fountain = new ItemFountain(Guid.NewGuid(), fountainBlueprint, new Mock<IRoom>().Object);

            fountain.Drink(10000000);

            Assert.AreEqual(fountainBlueprint.LiquidType, fountain.LiquidName);
            Assert.AreEqual(3, fountain.LiquidAmountMultiplier);
            Assert.AreEqual(int.MaxValue, fountain.LiquidLeft);
            Assert.IsFalse(fountain.IsEmpty);
            Assert.IsFalse(fountain.IsPoisoned);
        }

        // DrinkCountainer
        [TestMethod]
        public void DrinkContainer_Creation_Values()
        {
            ItemDrinkContainerBlueprint drinkContainerBlueprint = new ItemDrinkContainerBlueprint
            {
                Id = 1,
                Name = "drinkcontainer",
                ShortDescription = "DrinkContainerShort",
                Description = "DrinkContainerDesc",
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
                Id = 1,
                Name = "drinkcontainer",
                ShortDescription = "DrinkContainerShort",
                Description = "DrinkContainerDesc",
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
                Id = 1,
                Name = "drinkcontainer",
                ShortDescription = "DrinkContainerShort",
                Description = "DrinkContainerDesc",
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
                Id = 1,
                Name = "drinkcontainer",
                ShortDescription = "DrinkContainerShort",
                Description = "DrinkContainerDesc",
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
                Id = 1,
                Name = "drinkcontainer",
                ShortDescription = "DrinkContainerShort",
                Description = "DrinkContainerDesc",
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
                Id = 1,
                Name = "drinkcontainer",
                ShortDescription = "DrinkContainerShort",
                Description = "DrinkContainerDesc",
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
                Id = 1,
                Name = "drinkcontainer",
                ShortDescription = "DrinkContainerShort",
                Description = "DrinkContainerDesc",
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
                Id = 1,
                Name = "drinkcontainer",
                ShortDescription = "DrinkContainerShort",
                Description = "DrinkContainerDesc",
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