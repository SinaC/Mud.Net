using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.DamageArea)]
[Syntax("cast [spell]")]
[Help(
@"This spell inflicts damage on every enemy character in the room.
Beware that other characters who are not yet fighting may attack
you as a result!")]
[OneLineHelp("brings the power of earth to bear against your foes")]
public class Earthquake : NoTargetSpellBase
{
    private const string SpellName = "Earthquake";

    public Earthquake(ILogger<Earthquake> logger, IRandomManager randomManager) 
        : base(logger, randomManager)
    {
    }

    protected override void Invoke()
    {
        Caster.Send("The earth trembles beneath your feet!");
        Caster.Act(ActOptions.ToRoom, "{0:N} makes the earth tremble and shiver.", Caster);

        // Inform people in area
        foreach (var character in Caster.Room.Area.Characters.Where(x => x.Room != Caster.Room))
            character.Send("The earth trembles and shivers.");

        // Damage people in room
        var victims = Caster.Room.People.Where(x => x != Caster && !x.IsSafeSpell(Caster, true)).ToArray(); // clone to prevent modification issues
        foreach (var victim in victims)
        {
            var damage = victim.CharacterFlags.IsSet("Flying")
                ? 0 // no damage but starts fight
                : Level + RandomManager.Dice(2, 8);
            victim.AbilityDamage(Caster, damage, SchoolTypes.Bash, "earthquake", true);
            if (Caster.Fighting == null) // caster has been killed with some backlash damage
                break;
        }
    }
}
