using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Dispel)]
    public class Cancellation : DefensiveSpellBase
    {
        public const string SpellName = "Cancellation";

        private IDispelManager DispelManager { get; }

        public Cancellation(IRandomManager randomManager, IDispelManager dispelManager)
            : base(randomManager)
        {
            DispelManager = dispelManager;
        }

        protected override void Invoke()
        {
            if ((Caster is IPlayableCharacter && Victim is INonPlayableCharacter npcVictim && !Caster.CharacterFlags.IsSet("Charm") && npcVictim.Master == Caster)
                || (Caster is INonPlayableCharacter && Victim is IPlayableCharacter))
            {
                Caster.Send("You failed, try dispel magic.");
                return;
            }

            // unlike dispel magic, no save roll
            bool found = DispelManager.TryDispels(Level + 2, Victim);

            if (found)
                Caster.Send("Ok.");
            else
                Caster.Send("Spell failed.");
        }
    }
}
