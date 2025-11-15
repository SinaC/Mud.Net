using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Dispel)]
public class FaerieFog : NoTargetSpellBase
{
    private const string SpellName = "Faerie Fog";

    public FaerieFog(IRandomManager randomManager)
        : base(randomManager)
    {
    }

    protected override void Invoke()
    {
        Caster.Act(ActOptions.ToAll, "{0:N} conjure{0:v} a cloud of purple smoke.", Caster);
        foreach (var victim in Caster.Room.People.Where(x => x != Caster && !x.SavesSpell(Level, SchoolTypes.Other))) // && ich->invis_level <= 0
        {
            //victim.RemoveAuras(x => x.AbilityName == "Sneak" || x.AbilityName == "MassInvis" || x.AbilityName == "Invisibility", false);
            victim.RemoveAuras(x => x.Affects.OfType<ICharacterFlagsAffect>().Any(a => a.Modifier.IsSet("Invisible") || a.Modifier.IsSet("Sneak") || a.Modifier.IsSet("Hide")), false);
            // TODO: really needed ?
            //if (victim is INonPlayableCharacter)
            //    victim.RemoveBaseCharacterFlags(CharacterFlags.Hide | CharacterFlags.Invisible | CharacterFlags.Sneak);
            victim.Recompute();
            victim.Act(ActOptions.ToAll, "{0:N} is revealed!", victim);
        }
    }
}
