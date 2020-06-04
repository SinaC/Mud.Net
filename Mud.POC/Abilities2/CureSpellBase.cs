using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System.Linq;

namespace Mud.POC.Abilities2
{
    public abstract class CureSpellBase : DefensiveSpellBase
    {
        protected IAbilityManager AbilityManager { get; }

        protected CureSpellBase(IRandomManager randomManager, IWiznet wiznet, IAbilityManager abilityManager)
            : base(randomManager, wiznet)
        {
            AbilityManager = abilityManager;
        }

        protected override void Invoke()
        {
            CheckDispelReturnValues dispel = TryDispel(Level, Victim, ToCureAbilityName);
            switch (dispel)
            {
                case CheckDispelReturnValues.NotFound:
                    if (Victim == Caster)
                        Caster.Send(SelfNotFoundMsg);
                    else
                        Caster.Act(ActOptions.ToCharacter, NotSelfFoundMsg, Victim);
                    break;
                case CheckDispelReturnValues.FoundAndNotDispelled:
                    Caster.Send("Spell failed.");
                    break;
            }
        }

        protected abstract string ToCureAbilityName { get; }
        protected abstract string SelfNotFoundMsg { get; }
        protected abstract string NotSelfFoundMsg { get; }

        // TODO: refactoring, almost same code in DispelMagic and Cancellation
        protected CheckDispelReturnValues TryDispel(int dispelLevel, ICharacter victim, string abilityName) // was called check_dispel in Rom24
        {
            bool found = false;
            foreach (IAura aura in victim.Auras.Where(x => x.AbilityName == abilityName)) // no need to clone because at most one entry will be removed
            {
                if (!SavesDispel(dispelLevel, aura))
                {
                    victim.RemoveAura(aura, true); // RemoveAura will display DispelMessage
                    AbilityInfo abilityInfo = AbilityManager[aura.AbilityName];
                    string dispelRoomMessage = abilityInfo?.DispelRoomMessage;
                    if (!string.IsNullOrWhiteSpace(dispelRoomMessage))
                        victim.Act(ActOptions.ToRoom, dispelRoomMessage, victim);
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
