using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Faerie Fog", AbilityEffects.Dispel)]
    public class FaerieFog : NoTargetSpellBase
    {
        public FaerieFog(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            Caster.Act(ActOptions.ToAll, "{0:N} conjure{0:v} a cloud of purple smoke.", Caster);
            foreach (ICharacter victim in Caster.Room.People.Where(x => x != Caster && !x.SavesSpell(Level, SchoolTypes.Other))) // && ich->invis_level <= 0
            {
                victim.RemoveAuras(x => x.AbilityName == "Sneak" || x.AbilityName == "Mass Invis" || x.AbilityName == "Invisibility", false);
                // TODO: really needed ?
                //if (victim is INonPlayableCharacter)
                //    victim.RemoveBaseCharacterFlags(CharacterFlags.Hide | CharacterFlags.Invisible | CharacterFlags.Sneak);
                victim.Recompute();
                victim.Act(ActOptions.ToAll, "{0:N} is revealed!", victim);
            }
        }
    }
}
