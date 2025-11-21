using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Affects;

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
public class Poison : ItemOrOffensiveSpellBase
{
    private const string SpellName = "Poison";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }

    public Poison(ILogger<Poison> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
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
            AuraManager.AddAura(victim, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -2, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = new CharacterFlags(ServiceProvider, "Poison"), Operator = AffectOperators.Or },
                new PoisonDamageAffect());
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
            AuraManager.AddAura(weapon, SpellName, Caster, Level / 2, TimeSpan.FromMinutes(duration), AuraFlags.NoDispel, true,
                new ItemWeaponFlagsAffect { Modifier = new WeaponFlags(ServiceProvider, "Poison"), Operator = AffectOperators.Or });
            Caster.Act(ActOptions.ToCharacter, "{0} is coated with deadly venom.", weapon);
            return;
        }
        Caster.Act(ActOptions.ToCharacter, "You can't poison {0}.", item);
    }
}
