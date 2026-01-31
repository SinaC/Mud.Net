using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Dispel)]
[Syntax("cast [spell]")]
[Help(
@"This spell reveals all manner of invisible, hidden, and sneaking creatures in
the same room as you.")]
[OneLineHelp("reveals all hidden creatures in the room")]
public class FaerieFog : NoTargetSpellBase
{
    private const string SpellName = "Faerie Fog";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    public FaerieFog(ILogger<FaerieFog> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override void Invoke()
    {
        Caster.Act(ActOptions.ToAll, "{0:N} conjure{0:v} a cloud of purple smoke.", Caster);
        foreach (var victim in Caster.Room.People.Where(x => x != Caster && !x.SavesSpell(Level, SchoolTypes.Other))) // && ich->invis_level <= 0
        {
            //victim.RemoveAuras(x => StringCompareHelpers.StringEquals(x.AbilityName, "Sneak") || StringCompareHelpers.StringEquals(x.AbilityName, "MassInvis") || StringCompareHelpers.StringEquals(x.AbilityName, "Invisibility"), false);
            victim.RemoveAuras(x => x.Affects.OfType<ICharacterFlagsAffect>().Any(a => a.Modifier.IsSet("Invisible") || a.Modifier.IsSet("Sneak") || a.Modifier.IsSet("Hide")), false, true);
            // TODO: really needed ?
            //if (victim is INonPlayableCharacter)
            //    victim.RemoveBaseCharacterFlags(CharacterFlags.Hide | CharacterFlags.Invisible | CharacterFlags.Sneak);
            victim.Recompute();
            victim.Act(ActOptions.ToAll, "{0:N} is revealed!", victim);
        }
    }
}
