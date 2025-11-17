using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Domain;
using Mud.Server.Item;
using System;
using Moq;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Area;
using Mud.Server.Affects;
using Mud.Server.Flags;

namespace Mud.Server.Tests.Affects
{
    /*
    [TestClass]
    public class ItemWeaponAffectTests
    {
        [TestMethod]
        public void OneWeaponAddAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
            IItemWeapon weapon = new ItemWeapon(Guid.NewGuid(), new Blueprints.Item.ItemWeaponBlueprint { Id = 1, Name = "weapon1", ShortDescription = "weapon1short", Description = "weapon1desc", ItemFlags = new ItemFlags(), Flags = new WeaponFlags() }, room);
            IAura weaponAura = new Aura.Aura(null, null, AuraFlags.None, 10, TimeSpan.FromMinutes(20), new ItemWeaponFlagsAffect { Modifier = new WeaponFlags("Sharp"), Operator = AffectOperators.Add});
            weapon.AddAura(weaponAura, false);

            weapon.Recompute();

            Assert.IsTrue(weapon.BaseWeaponFlags.IsNone);
            Assert.IsTrue(weapon.WeaponFlags.IsSet("Sharp"));
        }

        [TestMethod]
        public void OneItemAddAffect_NoBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
            IItemWeapon weapon = new ItemWeapon(Guid.NewGuid(), new Blueprints.Item.ItemWeaponBlueprint { Id = 1, Name = "weapon1", ShortDescription = "weapon1short", Description = "weapon1desc", ItemFlags = new ItemFlags(), Flags = new WeaponFlags() }, room);
            IAura weaponAura = new Aura.Aura(null, null, AuraFlags.None, 10, TimeSpan.FromMinutes(20), new ItemFlagsAffect { Modifier = new ItemFlags("AntiEvil"), Operator = AffectOperators.Add });
            weapon.AddAura(weaponAura, false);

            weapon.Recompute();

            Assert.IsTrue(weapon.BaseWeaponFlags.IsNone);
            Assert.IsTrue(weapon.WeaponFlags.IsNone);
            Assert.IsTrue(weapon.BaseItemFlags.IsNone);
            Assert.IsTrue(weapon.ItemFlags.IsSet("AntiEvil"));
        }

        [TestMethod]
        public void MultipleEffect_MultipleBaseValue_Test()
        {
            IRoom room = new Room.Room(Guid.NewGuid(), new Blueprints.Room.RoomBlueprint { Id = 1, Name = "room1" }, new Mock<IArea>().Object);
            IItemWeapon weapon = new ItemWeapon(Guid.NewGuid(), new Blueprints.Item.ItemWeaponBlueprint { Id = 1, Name = "weapon1", ShortDescription = "weapon1short", Description = "weapon1desc", ItemFlags = new ItemFlags("AntiNeutral", "Bless"), Flags = new WeaponFlags("Flaming", "Sharp") }, room);
            IAura weaponAura = new Aura.Aura(null, null, AuraFlags.None, 10, TimeSpan.FromMinutes(20), 
                new ItemWeaponFlagsAffect { Modifier = new WeaponFlags("Sharp"), Operator = AffectOperators.Nor }, // remove weapon holy
                new ItemWeaponFlagsAffect { Modifier = new WeaponFlags("Frost"), Operator = AffectOperators.Assign }, // assign weapon frost
                new ItemFlagsAffect { Modifier = new ItemFlags("Dark"), Operator = AffectOperators.Add }, // add item dark
                new ItemFlagsAffect { Modifier = new ItemFlags("Bless"), Operator = AffectOperators.Or }, // or item bless (already present in base flags)
                new ItemFlagsAffect { Modifier = new ItemFlags("AntiGood"), Operator = AffectOperators.Nor } // remove antigood (was present in base flags)
                );
            weapon.AddAura(weaponAura, false);

            weapon.Recompute();

            Assert.IsTrue(weapon.BaseWeaponFlags.HasAll("Flaming", "Sharp"));
            Assert.IsTrue(weapon.WeaponFlags.IsSet("Frost"));
            Assert.IsTrue(weapon.BaseItemFlags.HasAll("AntiNeutral", "Bless"));
            Assert.IsTrue(weapon.ItemFlags.HasAll("AntiNeutral", "Bless", "Dark"));
        }
    }
    */
}
