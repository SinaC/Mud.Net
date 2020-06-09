using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class CureSpellBase : DefensiveSpellBase
    {
        protected IAbilityManager AbilityManager { get; }
        protected IDispelManager DispelManager { get; }

        protected CureSpellBase(IRandomManager randomManager, IAbilityManager abilityManager, IDispelManager dispelManager)
            : base(randomManager)
        {
            AbilityManager = abilityManager;
            DispelManager = dispelManager;
        }

        protected override void Invoke()
        {
            TryDispelReturnValues dispel = DispelManager.TryDispel(Level, Victim, ToCureAbilityName);
            switch (dispel)
            {
                case TryDispelReturnValues.NotFound:
                    if (Victim == Caster)
                        Caster.Send(SelfNotFoundMsg);
                    else
                        Caster.Act(ActOptions.ToCharacter, NotSelfFoundMsg, Victim);
                    break;
                case TryDispelReturnValues.FoundAndNotDispelled:
                    Caster.Send("Spell failed.");
                    break;
            }
        }

        protected abstract string ToCureAbilityName { get; }
        protected abstract string SelfNotFoundMsg { get; }
        protected abstract string NotSelfFoundMsg { get; }
    }
}
