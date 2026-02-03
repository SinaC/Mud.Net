using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability.Passive.Interfaces;

public interface IRegenerationPassive : IPassive
{
    decimal HitGainModifier(ICharacter user, decimal baseHitGain);
    decimal ResourceGainModifier(ICharacter user, ResourceKinds resourceKind, decimal baseResourceGain);
}
