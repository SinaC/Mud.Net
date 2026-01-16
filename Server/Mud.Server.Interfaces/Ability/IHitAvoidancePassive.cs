using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Ability;

public interface IHitAvoidancePassive : IPassive
{
    bool Avoid(ICharacter avoider, ICharacter aggressor, SchoolTypes damageType);
}
