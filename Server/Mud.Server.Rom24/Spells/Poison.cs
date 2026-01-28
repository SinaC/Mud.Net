using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Affects.Item;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff)]
[AbilityCharacterWearOffMessage("You feel less sick.")]
[AbilityItemWearOffMessage("The poison on {0} dries up.")]
[Syntax(
    "cast [spell] <victim>",
    "cast [spell] <object>")]
[Help(
@"This spell reduces the strength of the victim by two, as well as reducing the
victim's regeneration rate. It may also be used to poison food, drink, or
a weapon in a fashion similar to envenom ('help envenom'), but with 
drastically reduced effectiveness.")]
[OneLineHelp("weaker than plague, but often fatal")]
public class Poison : ItemOrOffensiveSpellBase
{
    private const string SpellName = "Poison";

    private IAuraManager AuraManager { get; }
    private IAffectManager AffectManager { get; }

    public Poison(ILogger<Poison> logger, IRandomManager randomManager, IAuraManager auraManager, IAffectManager affectManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
        AffectManager = affectManager;
    }

    protected override void Invoke(ICharacter victim)
    {
        if (victim.SavesSpell(Level, SchoolTypes.Poison))
        {
            victim.Act(ActOptions.ToRoom, "{0:N} turns slightly green, but it passes.", victim);
            victim.Send("You feel momentarily ill, but it passes.");
            return;
        }

        int duration = Level;
        var poisonAura = victim.GetAura(SpellName);
        if (poisonAura != null)
            poisonAura.Update(Level, TimeSpan.FromMinutes(duration));
        else
        {
            var poisonAffect = AffectManager.CreateInstance("Poison");
            AuraManager.AddAura(victim, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), new AuraFlags(), true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -2, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = new CharacterFlags("Poison"), Operator = AffectOperators.Or },
                poisonAffect,
                new CharacterRegenModifierAffect { Modifier = 4, Operator = AffectOperators.Divide });
        }
        victim.Send("You feel very sick.");
        victim.Act(ActOptions.ToRoom, "{0:N} looks very ill.", victim);
    }

    protected override void Invoke(IItem item)
    {
        // food/drink container
        if (item is IItemPoisonable poisonable)
        {
            if (poisonable.ItemFlags.HasAny("Bless", "BurnProof"))
            {
                Caster.Act(ActOptions.ToCharacter, "Your spell fails to corrupt {0}.", poisonable);
                return;
            }
            poisonable.Poison();
            Caster.Act(ActOptions.ToCharacter, "{0} is infused with poisonous vapors.", poisonable);
            return;
        }
        // weapon
        if (item is IItemWeapon weapon)
        {
            if (weapon.WeaponFlags.IsSet("Poison"))
            {
                Caster.Act(ActOptions.ToCharacter, "{0} is already envenomed.", weapon);
                return;
            }
            if (!weapon.WeaponFlags.IsNone)
            {
                Caster.Act(ActOptions.ToCharacter, "You can't seem to envenom {0}.", weapon);
                return;
            }
            int duration = Level / 8;
            AuraManager.AddAura(weapon, SpellName, Caster, Level / 2, TimeSpan.FromMinutes(duration), new AuraFlags("NoDispel"), true,
                new ItemWeaponFlagsAffect { Modifier = new WeaponFlags("Poison"), Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToCharacter, "{0} is coated with deadly venom.", weapon);
            return;
        }
        Caster.Act(ActOptions.ToCharacter, "You can't poison {0}.", item);
    }
}
