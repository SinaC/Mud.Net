using Mud.Container;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Mass Healing", AbilityEffects.HealingArea, PulseWaitTime = 36)]
    public class MassHealing : NoTargetSpellBase
    {
        private IAbilityManager AbilityManager { get; }

        public MassHealing(IRandomManager randomManager, IWiznet wiznet, IAbilityManager abilityManager)
            : base(randomManager, wiznet)
        {
            AbilityManager = abilityManager;
        }

        protected override void Invoke()
        {
            foreach (ICharacter victim in Caster.Room.People)
            {
                if ((Caster is IPlayableCharacter && victim is IPlayableCharacter)
                    || (Caster is INonPlayableCharacter && victim is INonPlayableCharacter))
                {
                    CastSpell("Heal", victim, Level);
                    CastSpell("Refresh", victim, Level);
                }
            }
        }

        private void CastSpell(string spellName, ICharacter victim, int level) // TODO: use level
        {
            // TODO: not a huge fan of following code
            AbilityInfo abilityInfo = AbilityManager[spellName];
            IAbilityAction abilityInstance = (IAbilityAction)DependencyContainer.Current.GetInstance(abilityInfo.AbilityExecutionType);
            AbilityActionInput abilityActionInput = new AbilityActionInput(abilityInfo, Caster, victim.Name, new CommandParameter(victim.Name, false));
            abilityInstance.Guards(abilityActionInput);
            abilityInstance.Execute(abilityActionInput);
        }
    }
}
