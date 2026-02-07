using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability.Spell;

public abstract class CharacterDebuffSpellBase : OffensiveSpellBase
{
    protected IAuraManager AuraManager { get; }

    protected CharacterDebuffSpellBase(ILogger<CharacterDebuffSpellBase> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (!CanAffect)
            return;
        var (level, duration, affects) = AuraInfo;
        AuraManager.AddAura(Victim, AbilityDefinition.Name, Caster, level, duration, true, affects);
        Victim.Act(ActOptions.ToCharacter, VictimAffectMessage, Caster);
        Victim.Act(ActOptions.ToRoom, RoomAffectMessage, Victim);
    }

    protected abstract SchoolTypes DebuffType { get; }
    protected abstract string VictimAffectMessage { get; }
    protected abstract string RoomAffectMessage { get; }

    protected abstract (int level, TimeSpan duration, IAffect[] affects) AuraInfo { get; }

    protected virtual bool CanAffect => Victim.GetAura(AbilityDefinition.Name) == null && !Victim.SavesSpell(Level, DebuffType);
}
