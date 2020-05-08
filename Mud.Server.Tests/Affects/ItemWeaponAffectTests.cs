﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Domain;
using Mud.Server.Aura;
using Mud.Server.Item;
using System;

namespace Mud.Server.Tests.Affects
{
    [TestClass]
    public class ItemWeaponAffectTests
    {
        [TestMethod]
        public void OneWeaponAddAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IItemWeapon weapon = new ItemWeapon(Guid.NewGuid(), new Blueprints.Item.ItemWeaponBlueprint { Id = 1, Name = "weapon1", ShortDescription = "weapon1short", Description = "weapon1desc", ItemFlags = ItemFlags.None, Flags = WeaponFlags.None }, room);
            IAura weaponAura = new Aura.Aura(null, null, AuraFlags.None, 10, TimeSpan.FromMinutes(20), new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Holy, Operator = AffectOperators.Add});
            weapon.AddAura(weaponAura, false);

            weapon.Recompute();

            Assert.AreEqual(WeaponFlags.None, weapon.BaseWeaponFlags);
            Assert.AreEqual(WeaponFlags.Holy, weapon.CurrentWeaponFlags);
        }

        [TestMethod]
        public void OneItemAddAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IItemWeapon weapon = new ItemWeapon(Guid.NewGuid(), new Blueprints.Item.ItemWeaponBlueprint { Id = 1, Name = "weapon1", ShortDescription = "weapon1short", Description = "weapon1desc", ItemFlags = ItemFlags.None, Flags = WeaponFlags.None }, room);
            IAura weaponAura = new Aura.Aura(null, null, AuraFlags.None, 10, TimeSpan.FromMinutes(20), new ItemFlagsAffect { Modifier = ItemFlags.AntiEvil, Operator = AffectOperators.Add });
            weapon.AddAura(weaponAura, false);

            weapon.Recompute();

            Assert.AreEqual(WeaponFlags.None, weapon.BaseWeaponFlags);
            Assert.AreEqual(WeaponFlags.None, weapon.CurrentWeaponFlags);
            Assert.AreEqual(ItemFlags.None, weapon.BaseItemFlags);
            Assert.AreEqual(ItemFlags.AntiEvil, weapon.ItemFlags);
        }

        [TestMethod]
        public void MultipleEffect_MultipleBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Area.Area("Area1", 1, 100, "builders", "credits"));
            IItemWeapon weapon = new ItemWeapon(Guid.NewGuid(), new Blueprints.Item.ItemWeaponBlueprint { Id = 1, Name = "weapon1", ShortDescription = "weapon1short", Description = "weapon1desc", ItemFlags = ItemFlags.AntiNeutral | ItemFlags.Bless, Flags = WeaponFlags.Flaming | WeaponFlags.Holy }, room);
            IAura weaponAura = new Aura.Aura(null, null, AuraFlags.None, 10, TimeSpan.FromMinutes(20), 
                new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Holy, Operator = AffectOperators.Nor }, // remove weapon holy
                new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Frost, Operator = AffectOperators.Assign }, // assign weapon frost
                new ItemFlagsAffect { Modifier = ItemFlags.Dark, Operator = AffectOperators.Add }, // add item dark
                new ItemFlagsAffect { Modifier = ItemFlags.Bless, Operator = AffectOperators.Or }, // or item bless (already present in base flags)
                new ItemFlagsAffect { Modifier = ItemFlags.AntiGood, Operator = AffectOperators.Nor } // remove antigood (was present in base flags)
                );
            weapon.AddAura(weaponAura, false);

            weapon.Recompute();

            Assert.AreEqual(WeaponFlags.Flaming | WeaponFlags.Holy, weapon.BaseWeaponFlags);
            Assert.AreEqual(WeaponFlags.Frost, weapon.CurrentWeaponFlags);
            Assert.AreEqual(ItemFlags.AntiNeutral | ItemFlags.Bless, weapon.BaseItemFlags);
            Assert.AreEqual(ItemFlags.AntiNeutral | ItemFlags.Bless | ItemFlags.Dark, weapon.ItemFlags);
        }
    }
}
