using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Domain;
using Mud.Server.Item;
using System;
using Moq;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Area;
using Mud.Server.Affect;

namespace Mud.Server.Tests.Affects
{
    [TestClass]
    public class ItemWeaponAffectTests
    {
        [TestMethod]
        public void OneWeaponAddAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
            IItemWeapon weapon = new ItemWeapon(Guid.NewGuid(), new Blueprints.Item.ItemWeaponBlueprint { Id = 1, Name = "weapon1", ShortDescription = "weapon1short", Description = "weapon1desc", ItemFlags = ItemFlags.None, Flags = WeaponFlags.None }, room);
            IAura weaponAura = new Aura.Aura(null, null, AuraFlags.None, 10, TimeSpan.FromMinutes(20), new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Holy, Operator = AffectOperators.Add});
            weapon.AddAura(weaponAura, false);

            weapon.Recompute();

            Assert.AreEqual(WeaponFlags.None, weapon.BaseWeaponFlags);
            Assert.AreEqual(WeaponFlags.Holy, weapon.WeaponFlags);
        }

        [TestMethod]
        public void OneItemAddAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
            IItemWeapon weapon = new ItemWeapon(Guid.NewGuid(), new Blueprints.Item.ItemWeaponBlueprint { Id = 1, Name = "weapon1", ShortDescription = "weapon1short", Description = "weapon1desc", ItemFlags = ItemFlags.None, Flags = WeaponFlags.None }, room);
            IAura weaponAura = new Aura.Aura(null, null, AuraFlags.None, 10, TimeSpan.FromMinutes(20), new ItemFlagsAffect { Modifier = ItemFlags.AntiEvil, Operator = AffectOperators.Add });
            weapon.AddAura(weaponAura, false);

            weapon.Recompute();

            Assert.AreEqual(WeaponFlags.None, weapon.BaseWeaponFlags);
            Assert.AreEqual(WeaponFlags.None, weapon.WeaponFlags);
            Assert.AreEqual(ItemFlags.None, weapon.BaseItemFlags);
            Assert.AreEqual(ItemFlags.AntiEvil, weapon.ItemFlags);
        }

        [TestMethod]
        public void MultipleEffect_MultipleBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
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
            Assert.AreEqual(WeaponFlags.Frost, weapon.WeaponFlags);
            Assert.AreEqual(ItemFlags.AntiNeutral | ItemFlags.Bless, weapon.BaseItemFlags);
            Assert.AreEqual(ItemFlags.AntiNeutral | ItemFlags.Bless | ItemFlags.Dark, weapon.ItemFlags);
        }
    }
}
