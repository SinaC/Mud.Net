using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
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
            if ((Caster is IPlayableCharacter && Victim is INonPlayableCharacter npcVictim && !Caster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcVictim.Master == Caster)
                || (Caster is INonPlayableCharacter && Victim is IPlayableCharacter))
            {
                Caster.Send("You failed, try dispel magic.");
                return;
            }

            // unlike dispel magic, no save roll
            bool found = DispelManager.TryDispels(Level + 2, Victim);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (found)
                Caster.Send("Ok.");
            else
                Caster.Send("Spell failed.");
        }
    }
}
