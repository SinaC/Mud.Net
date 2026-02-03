using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Ability.Passive.Interfaces;

public interface IHitAvoidancePassive : IPassive
{
    bool Avoid(ICharacter avoider, ICharacter aggressor, SchoolTypes damageType);
}
