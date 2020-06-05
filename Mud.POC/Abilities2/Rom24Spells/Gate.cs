using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Gate", AbilityEffects.Transportation)]
    public class Gate : TransportationSpellBase
    {
        public Gate(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            Caster.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", Caster);
            Caster.ChangeRoom(Victim.Room);
            Caster.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", Caster);
            Caster.AutoLook();

            // pets follows
            if (Caster is IPlayableCharacter pcCaster)
            {
                foreach (INonPlayableCharacter pet in pcCaster.Pets)
                {
                    pet.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", pet);
                    pet.ChangeRoom(Victim.Room);
                    pet.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", pet);
                    pet.AutoLook(); // TODO: needed ?
                }
            }
        }
    }
}
