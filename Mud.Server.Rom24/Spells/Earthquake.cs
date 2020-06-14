using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.DamageArea)]
    public class Earthquake : NoTargetSpellBase
    {
        public const string SpellName = "Earthquake";

        public Earthquake(IRandomManager randomManager) 
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            Caster.Send("The earth trembles beneath your feet!");
            Caster.Act(ActOptions.ToRoom, "{0:N} makes the earth tremble and shiver.", Caster);

            // Inform people in area
            foreach (ICharacter character in Caster.Room.Area.Characters.Where(x => x.Room != Caster.Room))
                character.Send("The earth trembles and shivers.");

            // Damage people in room
            foreach (ICharacter victim in Caster.Room.People.Where(x => x != Caster && !x.IsSafeSpell(Caster, true)))
            {
                int damage = victim.CharacterFlags.HasFlag(CharacterFlags.Flying)
                    ? 0 // no damage but starts fight
                    : Level + RandomManager.Dice(2, 8);
                victim.AbilityDamage(Caster, damage, SchoolTypes.Bash, "earthquake", true);
            }
        }
    }

}
