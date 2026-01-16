using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Ability.Passive;

public abstract class RegenerationPassiveBase : PassiveBase, IRegenerationPassive
{
    protected RegenerationPassiveBase(ILogger<RegenerationPassiveBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    public virtual decimal HitGainModifier(ICharacter user, decimal baseHitGain) => 0;

    public virtual decimal ResourceGainModifier(ICharacter user, ResourceKinds resourceKind, decimal baseResourceGain) => 0;
}
