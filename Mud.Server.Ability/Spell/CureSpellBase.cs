using Microsoft.Extensions.Logging;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell;

public abstract class CureSpellBase : DefensiveSpellBase
{
    protected IAbilityManager AbilityManager { get; }
    protected IDispelManager DispelManager { get; }

    protected CureSpellBase(ILogger<CureSpellBase> logger, IRandomManager randomManager, IAbilityManager abilityManager, IDispelManager dispelManager)
        : base(logger, randomManager)
    {
        AbilityManager = abilityManager;
        DispelManager = dispelManager;
    }

    protected override void Invoke()
    {
        var dispel = DispelManager.TryDispel(Level, Victim, ToCureAbilityName);
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
