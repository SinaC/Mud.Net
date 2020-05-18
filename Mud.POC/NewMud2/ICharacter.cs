using System.Collections.Generic;

namespace Mud.POC.NewMud2
{
    public interface ICharacter : IEntity
    {
        IRoom Room { get; }
        IEnumerable<IItem> Inventory { get; }
        IEnumerable<IItem> Equipments { get; }
        IEnumerable<ICharacter> PartyMembers { get; }

        ICharacter TargetEnemy { get; }
        ICharacter TargetAlly { get; }

        int Level { get; }
        int HitPoints { get; }

        void SetTargetEnemy(ICharacter enemy);
        void SetTargetAlly(ICharacter ally);

        void Send(string format, params object[] args);
        void Act(ActTargets actTarget, string format, params object[] args);

        void MultiHit(ICharacter victim);
        void AbilityDamage(IDamageAbility ability, ICharacter victim, int damage, DamageTypes damageType);
        void Heal(IHealAbility ability, ICharacter victim, int heal);

        bool IsSafe(ICharacter victim);
        bool SavesSpell(int level, DamageTypes damageType);
    }

    public enum ActTargets
    {
        ToCharacter,
        ToRoom
    }
}
