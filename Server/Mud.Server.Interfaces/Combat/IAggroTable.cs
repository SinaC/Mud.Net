using Mud.Server.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Combat;

public interface IAggroTable : IReadOnlyAggroTable
{
    AggroResults OnAggroGeneratedByEnemy(ICharacter aggroGenerator, long delta);
    AggroResults OnDamageDealtByEnemy(ICharacter damageDealer, int amount);
    AggroResults OnHealByEnemy(ICharacter healer, int amount);
    AggroResults OnEnemyKilled(ICharacter enemy);
    AggroResults OnEnemyFled(ICharacter enemy);
    AggroResults OnTaunt(ICharacter enemy);
    AggroResults OnStopFight(ICharacter enemy);

    void Clear();
}
