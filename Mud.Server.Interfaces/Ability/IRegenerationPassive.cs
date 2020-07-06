using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Ability
{
    public interface IRegenerationPassive : IPassive
    {
        int HitGainModifier(ICharacter user, int baseHitGain);
        int ResourceGainModifier(ICharacter user, ResourceKinds resourceKind, int baseResourceGain);
    }
}
