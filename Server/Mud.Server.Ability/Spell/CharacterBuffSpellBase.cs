using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Ability.Spell;

public abstract class CharacterBuffSpellBase : DefensiveSpellBase
{
    protected IAuraManager AuraManager { get; }

    protected CharacterBuffSpellBase(ILogger<CharacterBuffSpellBase> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (IsAffected)
            return;
        var (level, duration, affects) = AuraInfo;
        AuraManager.AddAura(Victim, AbilityDefinition.Name, Caster, level, duration, AuraFlags.None, true, affects);
        Victim.Act(ActOptions.ToCharacter, VictimAffectMessage, Caster);
        if (Victim != Caster)
            Caster.Act(ActOptions.ToCharacter, CasterAffectMessage, Victim);
    }

    protected abstract string SelfAlreadyAffectedMessage { get; }
    protected abstract string NotSelfAlreadyAffectedMessage { get; }
    protected abstract string VictimAffectMessage { get; }
    protected abstract string CasterAffectMessage { get; }

    protected abstract (int level, TimeSpan duration, IAffect[] affects) AuraInfo { get; }

    protected virtual bool IsAffected
    {
        get
        {
            if (Victim.GetAura(AbilityDefinition.Name) != null)
            {
                if (Victim != Caster)
                    Caster.Send(SelfAlreadyAffectedMessage);
                else
                    Caster.Act(ActOptions.ToCharacter, NotSelfAlreadyAffectedMessage, Victim);
                return true;
            }

            return false;
        }
    }
}
