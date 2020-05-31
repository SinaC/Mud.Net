using Mud.POC.Abilities2.Interfaces;
using Mud.POC.Abilities2.Rom24Spells;
using Mud.Server.Common;
using System.Linq;

namespace Mud.POC.Abilities2
{
    public abstract class CureSpellBase : DefensiveSpellBase
    {
        public CureSpellBase(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        #region IAbility

        public override AbilityEffects Effects => AbilityEffects.Cure;

        #endregion

        #region DefensiveSpellBase

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            CheckDispelReturnValues dispel = TryDispel(level, victim, ToCureAbilityName);
            switch (dispel)
            {
                case CheckDispelReturnValues.NotFound:
                    if (victim == caster)
                        caster.Send(SelfNotFoundMsg);
                    else
                        caster.Act(ActOptions.ToCharacter, NotSelfFoundMsg, victim);
                    break;
                case CheckDispelReturnValues.FoundAndNotDispelled:
                    caster.Send("Spell failed.");
                    break;
            }
        }

        #endregion

        protected abstract string ToCureAbilityName { get; }
        protected abstract string SelfNotFoundMsg { get; }
        protected abstract string NotSelfFoundMsg { get; }

        protected CheckDispelReturnValues TryDispel(int dispelLevel, ICharacter victim, string abilityName) // was called check_dispel in Rom24
        {
            bool found = false;
            foreach (IAura aura in victim.Auras.Where(x => x.Ability.Name == abilityName)) // no need to clone because at most one entry will be removed
            {
                if (!SavesDispel(dispelLevel, aura))
                {
                    victim.RemoveAura(aura, true); // RemoveAura will display DispelMessage
                    if (aura.Ability != null && aura.Ability is IAbilityDispellable dispellableAbility && !string.IsNullOrWhiteSpace(dispellableAbility.DispelRoomMessage))
                        victim.Act(ActOptions.ToRoom, dispellableAbility.DispelRoomMessage, victim);
                    return CheckDispelReturnValues.Dispelled; // stop at first aura dispelled
                }
                else
                    aura.DecreaseLevel();
                found = true;
            }
            return found
                ? CheckDispelReturnValues.FoundAndNotDispelled
                : CheckDispelReturnValues.NotFound;
        }
        protected enum CheckDispelReturnValues
        {
            NotFound,
            Dispelled,
            FoundAndNotDispelled
        }
    }
}
