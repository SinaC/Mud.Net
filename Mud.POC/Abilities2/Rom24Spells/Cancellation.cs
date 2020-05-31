using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class Cancellation : OffensiveSpellBase
    {
        public override int Id => 8;

        public override string Name => "Cancellation";

        public override AbilityEffects Effects => AbilityEffects.Dispel;

        public Cancellation(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            if ((caster is IPlayableCharacter && victim is INonPlayableCharacter npcVictim && !caster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcVictim.Master == caster)
                || (caster is INonPlayableCharacter && victim is IPlayableCharacter))
            {
                caster.Send("You failed, try dispel magic.");
                return;
            }

            // unlike dispel magic, no save roll
            bool found = TryDispels(level + 2, victim);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (found)
                caster.Send("Ok.");
            else
                caster.Send("Spell failed.");
        }

        protected override AbilityTargetResults SetTargets(ICharacter caster, string rawParameters, params CommandParameter[] parameters) => AbilityTargetResults.Ok;

        private bool TryDispels(int dispelLevel, ICharacter victim) // dispels every spells
        {
            bool found = false;
            IReadOnlyCollection<IAura> clone = new ReadOnlyCollection<IAura>(victim.Auras.Where(x => x.IsValid).ToList());
            foreach (IAura aura in clone)
            {
                if (!SavesDispel(dispelLevel, aura))
                {
                    found = true;
                    victim.RemoveAura(aura, false); // RemoveAura will display DispelMessage
                    if (aura.Ability != null && aura.Ability is IAbilityDispellable dispellableAbility && !string.IsNullOrWhiteSpace(dispellableAbility.DispelRoomMessage))
                        victim.Act(ActOptions.ToRoom, dispellableAbility.DispelRoomMessage, victim);
                }
                else
                    aura.DecreaseLevel();
            }
            if (found)
                victim.Recompute();
            return found;
        }
    }
}
