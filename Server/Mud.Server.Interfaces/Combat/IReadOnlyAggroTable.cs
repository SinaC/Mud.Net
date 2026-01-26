using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Combat;

public interface IReadOnlyAggroTable
{
    IReadOnlyDictionary<ICharacter, long> AggroByEnemy { get; }
    ICharacter? GetEnemyInRoom(IRoom room);
}
