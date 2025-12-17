using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Ability;

public interface IRegenerationPassive : IPassive
{
    decimal HitGainModifier(ICharacter user, decimal baseHitGain);
    decimal ResourceGainModifier(ICharacter user, ResourceKinds resourceKind, decimal baseResourceGain);
}
