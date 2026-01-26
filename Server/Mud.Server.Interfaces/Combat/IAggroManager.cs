using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Interfaces.Combat;

public interface IAggroManager
{
    IReadOnlyAggroTable? GetAggroTable(INonPlayableCharacter npc);
    ICharacter? ChooseEnemy(INonPlayableCharacter npc);

    void OnStartFight(ICharacter source, ICharacter victim);
    void OnDeath(ICharacter victim);
    void OnFlee(ICharacter character, IRoom wasInRoom);
    void OnReceiveDamage(ICharacter source, ICharacter victim, int damage);
    void OnHeal(ICharacter source, ICharacter target, int heal);
    void OnTaunt(ICharacter source, ICharacter victim);
    void OnRemove(ICharacter character);

    void DecreaseAggroOfEnemiesIfNotInSameRoom(ICharacter character);

    void Clear(ICharacter character);
}