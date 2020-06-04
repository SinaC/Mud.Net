using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Cancellation", AbilityEffects.Dispel)]
    public class Cancellation : DefensiveSpellBase
    {
        private IAbilityManager AbilityManager { get; }
        public Cancellation(IRandomManager randomManager, IWiznet wiznet, IAbilityManager abilityManager)
            : base(randomManager, wiznet)
        {
            AbilityManager = abilityManager;
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
            bool found = TryDispels(Level + 2, Victim);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (found)
                Caster.Send("Ok.");
            else
                Caster.Send("Spell failed.");
        }

        // TODO: refactor, same code in DispelMagic
        private bool TryDispels(int dispelLevel, ICharacter victim) // dispels every spells
        {
            bool found = false;
            IReadOnlyCollection<IAura> clone = new ReadOnlyCollection<IAura>(victim.Auras.Where(x => x.IsValid).ToList());
            foreach (IAura aura in clone)
            {
                if (!SavesDispel(dispelLevel, aura))
                {
                    found = true;
                    victim.RemoveAura(aura, false); // RemoveAura will display WearOff message
                    if (aura.AbilityName != null)
                    {
                        AbilityInfo abilityInfo = AbilityManager[aura.AbilityName];
                        string dispelRoomMessage = abilityInfo?.DispelRoomMessage;
                        if (!string.IsNullOrWhiteSpace(dispelRoomMessage))
                            victim.Act(ActOptions.ToRoom, dispelRoomMessage, victim);
                    }
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
