using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class DispelMagic : OffensiveSpellBase
    {
        public override int Id => 39;
        public override string Name => "Dispel Magic";
        public override AbilityEffects Effects => AbilityEffects.Dispel;

        public DispelMagic(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            if (victim.SavesSpell(level, SchoolTypes.Other))
            {
                victim.Send("You feel a brief tingling sensation.");
                caster.Send("You failed.");
                return;
            }

            bool found = TryDispels(level, victim);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (found)
                caster.Send("Ok.");
            else
                caster.Send("Spell failed.");
        }

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
