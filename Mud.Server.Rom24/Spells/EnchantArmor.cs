using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Enchantment, PulseWaitTime = 24)]
[Syntax("cast [spell] <armor>")]
[Help(
@"The enchant armor spell imbues armor with powerful protective magics. It is
not nearly as reliable as enchant weapon, being far more prone to destructive
effects.  Each succesful enchant increases the plus of the armor by 1 or 2
points, and raises its level by one.")]
public class EnchantArmor : ItemInventorySpellBase<IItemArmor>
{
    private const string SpellName = "Enchant Armor";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }
    private IItemManager ItemManager { get; }

    public EnchantArmor(ILogger<EnchantArmor> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
        ItemManager = itemManager;
    }

    protected override string InvalidItemTypeMsg => "That isn't an armor.";

    protected override void Invoke()
    {
        IItemArmor armor = Item;
        //if (item.EquippedBy == null)
        //{
        //    caster.Send("The item must be carried to be enchanted.");
        //    return;
        //}

        IAura? existingAura = null;
        int fail = 25; // base 25% chance of failure

        // find existing bonuses
        foreach (var aura in armor.Auras)
        {
            if (aura.AbilityName == SpellName)
                existingAura = aura;
            bool found = false;
            foreach (var characterAttributeAffect in aura.Affects.OfType<CharacterAttributeAffect>().Where(x => x.Location == CharacterAttributeAffectLocations.AllArmor))
            {
                fail += 5 * (characterAttributeAffect.Modifier * characterAttributeAffect.Modifier);
                found = true;
            }
            if (!found) // things get a little harder
                fail += 20;
        }
        // apply other modifiers
        fail -= Level;
        if (armor.ItemFlags.IsSet("Bless"))
            fail -= 15;
        if (armor.ItemFlags.IsSet("Glowing"))
            fail -= 5;
        fail = fail.Range(5, 85);
        // the moment of truth
        int result = RandomManager.Range(1, 100);
        if (result < fail / 5) // item destroyed
        {
            Caster.Act(ActOptions.ToAll, "{0} flares blindingly... and evaporates!", armor);
            ItemManager.RemoveItem(armor);
            return;
        }
        if (result < fail / 3) // item disenchanted
        {
            Caster.Act(ActOptions.ToCharacter, "{0} glows brightly, then fades...oops.!", armor);
            Caster.Act(ActOptions.ToRoom, "{0} glows brightly, then fades.", armor);
            armor.Disenchant();
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
            Caster.Act(ActOptions.ToAll, "{0} shimmers with a gold aura.", armor);
            amount = -1;
        }
        else // exceptional enchant
        {
            Caster.Act(ActOptions.ToAll, "{0} glows a brillant gold!", armor);
            addGlowing = true;
            amount = -2;
        }
        armor.IncreaseLevel();

        if (existingAura != null)
        {
            existingAura.AddOrUpdateAffect(
                    x => x.Location == CharacterAttributeAffectLocations.AllArmor,
                    () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = amount, Operator = AffectOperators.Add },
                    x => x.Modifier += amount);
            existingAura.AddOrUpdateAffect(
                    x => x.Modifier.IsSet( "Magic"),
                    () => new ItemFlagsAffect { Modifier = new ItemFlags(ServiceProvider, "Magic"), Operator = AffectOperators.Or },
                    _ => { });
            if (addGlowing)
                existingAura.AddOrUpdateAffect(
                   x => x.Modifier.IsSet("Glowing"),
                   () => new ItemFlagsAffect { Modifier = new ItemFlags(ServiceProvider, "Glowing"), Operator = AffectOperators.Or },
                   _ => { });
        }
        else
        {
            List<IAffect> affects =
            [
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = amount, Operator = AffectOperators.Add },
                new ItemFlagsAffect { Modifier = new ItemFlags(ServiceProvider, "Magic"), Operator = AffectOperators.Or }
            ];
            if (addGlowing)
                affects.Add(new ItemFlagsAffect { Modifier = new ItemFlags(ServiceProvider, "Glowing"), Operator = AffectOperators.Or });
            AuraManager.AddAura(armor, SpellName, Caster, Level, Pulse.Infinite, AuraFlags.Permanent, false, affects.ToArray());
        }
        armor.Recompute();
    }
}
