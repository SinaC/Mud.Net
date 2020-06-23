using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Transportation)]
    public class Gate : TransportationSpellBase
    {
        public const string SpellName = "Gate";
            
        public Gate(IRandomManager randomManager, ICharacterManager characterManager)
            : base(randomManager, characterManager)
        {
        }

        protected override void Invoke()
        {
            Caster.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", Caster);
            Caster.ChangeRoom(Victim.Room);
            Caster.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", Caster);
            AutoLook(Caster);

            // pets follows
            if (Caster is IPlayableCharacter pcCaster)
            {
                foreach (INonPlayableCharacter pet in pcCaster.Pets)
                {
                    pet.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", pet);
                    pet.ChangeRoom(Victim.Room);
                    pet.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", pet);
                    AutoLook(pet); // TODO: needed ?
                }
            }
        }
    }
}
