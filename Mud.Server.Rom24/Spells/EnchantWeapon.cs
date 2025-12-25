using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Affects.Item;
using Mud.Server.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Enchantment, PulseWaitTime = 24, NotInCombat = true)]
[Syntax("cast [spell] <weapon>")]
[Help(
@"This spell magically enchants a weapon, increasing its to-hit and to-dam
bonuses by one or two points.  Multiple enchants may be cast, but as the
weapon grows more and more powerful, it is more likely to be drained or
destroyed by the magic.  Also, every successful enchant increases the level
of the weapon by one...and there is no turning back.")]
[OneLineHelp("increases the hit and damage bonuses of a weapon")]
public class EnchantWeapon : ItemInventorySpellBase<IItemWeapon>
{
    private const string SpellName = "Enchant Weapon";

    private IAuraManager AuraManager { get; }
    private IItemManager ItemManager { get; }

    public EnchantWeapon(ILogger<EnchantWeapon> logger, IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
        ItemManager = itemManager;
    }

    protected override string InvalidItemTypeMsg => "This is not a weapon.";

    protected override void Invoke()
    {
        IItemWeapon weapon = Item;
        //if (weapon.EquippedBy == null)
        //{
        //    Caster.Send("The item must be carried to be enchanted.");
        //    return;
        //}

        IAura? existingAura = null;
        int fail = 25; // base 25% chance of failure

        // find existing bonuses
        foreach (IAura aura in weapon.Auras)
        {
            if (StringCompareHelpers.StringEquals(aura.AbilityName, SpellName))
                existingAura = aura;
            bool found = false;
            foreach (CharacterAttributeAffect characterAttributeAffect in aura.Affects.OfType<CharacterAttributeAffect>().Where(x => x.Location == CharacterAttributeAffectLocations.HitRoll || x.Location == CharacterAttributeAffectLocations.DamRoll))
            {
                fail += 5 * (characterAttributeAffect.Modifier * characterAttributeAffect.Modifier);
                found = true;
            }
            if (!found) // things get a little harder
                fail += 20;
        }
        // apply other modifiers
        fail -= 3 * Level / 2;
        if (weapon.ItemFlags.IsSet("Bless"))
            fail -= 15;
        if (weapon.ItemFlags.IsSet("Glowing"))
            fail -= 5;
        fail = Math.Clamp(fail, 5, 95);
        // the moment of truth
        int result = RandomManager.Range(1, 100);
        if (result < fail / 5) // item destroyed
        {
            Caster.Act(ActOptions.ToAll, "{0:N} flares blindingly... and evaporates!", weapon);
            ItemManager.RemoveItem(weapon);
            return;
        }
        if (result < fail / 3) // item disenchanted
        {
            Caster.Act(ActOptions.ToCharacter, "{0} glows brightly, then fades...oops.!", weapon);
            Caster.Act(ActOptions.ToRoom, "{0:N} glows brightly, then fades.", weapon);
            weapon.Disenchant();
            return;
        }
        if (result <= fail) // failed, no bad result
        {
            Caster.Send("Nothing seemed to happen.");
            return;
        }
        int amount;
        bool addGlowing = false;
        if (result <= (90 - Level / 5)) // success
        {
            Caster.Act(ActOptions.ToAll, "{0:N} glows blue.", weapon);
            amount = 1;
        }
        else // exceptional enchant
        {
            Caster.Act(ActOptions.ToAll, "{0:N} glows a brillant blue!", weapon);
            addGlowing = true;
            amount = 2;
        }
        weapon.IncreaseLevel();
        if (existingAura != null)
        {
            existingAura.AddOrUpdateAffect(
                    x => x.Location == CharacterAttributeAffectLocations.HitRoll,
                    () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = amount, Operator = AffectOperators.Add },
                    x => x.Modifier += amount);
            existingAura.AddOrUpdateAffect(
                    x => x.Location == CharacterAttributeAffectLocations.DamRoll,
                    () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = amount, Operator = AffectOperators.Add },
                    x => x.Modifier += amount);
            existingAura.AddOrUpdateAffect(
                    x => x.Modifier.IsSet("Magic"),
                    () => new ItemFlagsAffect { Modifier = new ItemFlags("Magic"), Operator = AffectOperators.Or },
                    _ => { });
            if (addGlowing)
                existingAura.AddOrUpdateAffect(
                   x => x.Modifier.IsSet("Glowing"),
                   () => new ItemFlagsAffect { Modifier = new ItemFlags("Glowing"), Operator = AffectOperators.Or },
                   _ => { });
        }
        else
        {
            List<IAffect> affects =
            [
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.HitRoll, Modifier = amount, Operator = AffectOperators.Add },
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.DamRoll, Modifier = amount, Operator = AffectOperators.Add },
                new ItemFlagsAffect { Modifier = new ItemFlags("Magic"), Operator = AffectOperators.Or }
            ];
            if (addGlowing)
                affects.Add(new ItemFlagsAffect { Modifier = new ItemFlags("Glowing"), Operator = AffectOperators.Or });
            AuraManager.AddAura(weapon, SpellName, Caster, Level, Pulse.Infinite, AuraFlags.Permanent, false, affects.ToArray());
        }
        weapon.Recompute();
    }
}
