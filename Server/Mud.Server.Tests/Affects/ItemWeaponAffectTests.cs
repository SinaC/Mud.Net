using Mud.Domain;
using Mud.Server.Affects.Item;
using Mud.Server.Domain;
using Mud.Flags;

namespace Mud.Server.Tests.Affects;

[TestClass]
public class ItemWeaponAffectTests : TestBase
{
    [TestMethod]
    public void OneWeaponAddAffect_NoBaseValue()
    {
        var room = GenerateRoom("");
        var weapon = GenerateWeapon("", "", room);
        var weaponAura = new Aura.Aura(null!, null!, new AuraFlags(), 10, TimeSpan.FromMinutes(20),  new ItemWeaponFlagsAffect { Modifier = new WeaponFlags("Sharp"), Operator = AffectOperators.Add});
        weapon.AddAura(weaponAura, false);

        weapon.Recompute();

        Assert.IsTrue(weapon.BaseWeaponFlags.IsNone);
        Assert.IsTrue(weapon.WeaponFlags.IsSet("Sharp"));
    }

    [TestMethod]
    public void OneItemAddAffect_NoBaseValue()
    {
        var room = GenerateRoom("");
        var weapon = GenerateWeapon("", "", room);
        var weaponAura = new Aura.Aura(null!, null!, new AuraFlags(), 10, TimeSpan.FromMinutes(20), new ItemFlagsAffect { Modifier = new ItemFlags("AntiEvil"), Operator = AffectOperators.Add });
        weapon.AddAura(weaponAura, false);

        weapon.Recompute();

        Assert.IsTrue(weapon.BaseWeaponFlags.IsNone);
        Assert.IsTrue(weapon.WeaponFlags.IsNone);
        Assert.IsTrue(weapon.BaseItemFlags.IsNone);
        Assert.IsTrue(weapon.ItemFlags.IsSet("AntiEvil"));
    }

    [TestMethod]
    public void MultipleEffects_MultipleBaseValue()
    {
        var room = GenerateRoom("");
        var weapon = GenerateWeapon("AntiNeutral,Bless", "Flaming,Sharp", room);
        var weaponAura = new Aura.Aura(null!, null!, new AuraFlags(), 10, TimeSpan.FromMinutes(20), 
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

    [TestMethod]
    public void MultipleEffectsOnContainingRoom()
    {
        var room = GenerateRoom("");
        var roomAura = new Aura.Aura(null!, null!, new AuraFlags(), 10, TimeSpan.FromMinutes(20),
            new ItemFlagsAffect { Modifier = new ItemFlags("AntiGood"), Operator = AffectOperators.Or },
            new ItemWeaponFlagsAffect { Modifier = new WeaponFlags("Sharp"), Operator = AffectOperators.Add });
        room.AddAura(roomAura, false);
        var weapon = GenerateWeapon("", "", room);

        weapon.Recompute();

        Assert.IsTrue(weapon.BaseItemFlags.IsNone);
        Assert.IsTrue(weapon.ItemFlags.IsSet("AntiGood"));
        Assert.IsTrue(weapon.BaseWeaponFlags.IsNone);
        Assert.IsTrue(weapon.WeaponFlags.IsSet("Sharp"));
    }
}
