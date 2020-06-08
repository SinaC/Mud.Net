using Mud.Server.Common;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.POC.Abilities2
{
    public abstract class OffensiveSpellBase : SpellBase
    {
        protected ICharacter Victim { get; set; }

        protected OffensiveSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        public override void Execute()
        {
            base.Execute();

            INonPlayableCharacter npcVictim = Victim as INonPlayableCharacter;
            if (Victim != Caster
                && npcVictim?.Master != Caster)
            {
                // TODO: not sure why we loop on people in caster room
                // TODO: we could just check if victim is still in the room and not fighting
                IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(Caster.Room.People.ToList());
                foreach (ICharacter victim in clone)
                {
                    if (victim == Victim && victim.Fighting == null)
                    {
                        // TODO: check_killer
                        victim.MultiHit(Caster);
                        break;
                    }
                }
            }
        }

        protected override string SetTargets(AbilityActionInput abilityActionInput)
        {
            if (abilityActionInput.Parameters.Length < 1)
            {
                Victim = Caster.Fighting;
                if (Victim == null)
                    return "Cast the spell on whom?";
            }
            else
                Victim = FindHelpers.FindByName(Caster.Room.People, abilityActionInput.Parameters[0]);
            if (Victim == null)
                return "They aren't here.";
            if (Caster is IPlayableCharacter)
            {
                if (Caster != Victim && Victim.IsSafe(Caster))
                    return "Not on that victim.";
                // TODO: check_killer
            }
            if (Caster is INonPlayableCharacter npcCaster && npcCaster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcCaster.Master == Victim)
                return "You can't do that on your own follower.";
            // victim found
            return null;
        }
    }
}
