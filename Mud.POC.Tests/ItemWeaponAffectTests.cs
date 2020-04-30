using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mud.Domain;
using Mud.POC.Affects;

namespace Mud.POC.Tests
{
    [TestClass]
    public class ItemWeaponAffectTests
    {
        [TestMethod]
        public void OneWeaponAddAffect_NoBaseValue_Test()
        {
            ItemWeapon weapon = new ItemWeapon("weapon1", null, null, ItemFlags.None, WeaponFlags.None);
            IAura weaponAura = new Aura(null, null, Affects.AuraFlags.None, 10, 20, new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Holy, Operator = Affects.AffectOperators.Add});
            weapon.AddAura(weaponAura);

            weapon.Recompute();

            Assert.AreEqual(WeaponFlags.None, weapon.BaseWeaponFlags);
            Assert.AreEqual(WeaponFlags.Holy, weapon.CurrentWeaponFlags);
        }

        [TestMethod]
        public void OneItemAddAffect_NoBaseValue_Test()
        {
            ItemWeapon weapon = new ItemWeapon("weapon1", null, null, ItemFlags.None, WeaponFlags.None);
            IAura weaponAura = new Aura(null, null, Affects.AuraFlags.None, 10, 20, new ItemFlagsAffect { Modifier = ItemFlags.AntiEvil, Operator = Affects.AffectOperators.Add });
            weapon.AddAura(weaponAura);

            weapon.Recompute();

            Assert.AreEqual(WeaponFlags.None, weapon.BaseWeaponFlags);
            Assert.AreEqual(WeaponFlags.None, weapon.CurrentWeaponFlags);
            Assert.AreEqual(ItemFlags.None, weapon.BaseItemFlags);
            Assert.AreEqual(ItemFlags.AntiEvil, weapon.CurrentItemFlags);
        }

        [TestMethod]
        public void MultipleEffect_MultipleBaseValue_Test()
        {
            ItemWeapon weapon = new ItemWeapon("weapon1", null, null, ItemFlags.AntiNeutral | ItemFlags.Bless, WeaponFlags.Flaming | WeaponFlags.Holy);
            IAura weaponAura = new Aura(null, null, Affects.AuraFlags.None, 10, 20, 
                new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Holy, Operator = Affects.AffectOperators.Nor }, // remove weapon holy
                new ItemWeaponFlagsAffect { Modifier = WeaponFlags.Frost, Operator = Affects.AffectOperators.Assign }, // assign weapon frost
                new ItemFlagsAffect { Modifier = ItemFlags.Dark, Operator = Affects.AffectOperators.Add }, // add item dark
                new ItemFlagsAffect { Modifier = ItemFlags.Bless, Operator = Affects.AffectOperators.Or }, // or item bless (already present in base flags)
                new ItemFlagsAffect { Modifier = ItemFlags.AntiGood, Operator = Affects.AffectOperators.Nor } // remove antigood (was present in base flags)
                );
            weapon.AddAura(weaponAura);

            weapon.Recompute();

            Assert.AreEqual(WeaponFlags.Flaming | WeaponFlags.Holy, weapon.BaseWeaponFlags);
            Assert.AreEqual(WeaponFlags.Frost, weapon.CurrentWeaponFlags);
            Assert.AreEqual(ItemFlags.AntiNeutral | ItemFlags.Bless, weapon.BaseItemFlags);
            Assert.AreEqual(ItemFlags.AntiNeutral | ItemFlags.Bless | ItemFlags.Dark, weapon.CurrentItemFlags);
        }
    }
}
