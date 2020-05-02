using System.Collections.Generic;

namespace Mud.POC.Abilities
{
    public interface ICharacter : IEntity
    {
        int Level { get; }

        IRoom Room { get; }

        ICharacter Fighting { get; }

        IEnumerable<IItem> Inventory { get; }

        IEnumerable<IItem> Equipments { get; }

        IEnumerable<KnownAbility> KnownAbilities { get; }

        void Send(string msg, params string[] args);
    }
}
