using System.Collections.Generic;

namespace Mud.POC.Abilities
{
    public interface ICharacter : IEntity
    {
        int Level { get; }

        IClass Class { get; }

        IRace Race { get; }

        IRoom Room { get; }

        ICharacter Fighting { get; }

        IEnumerable<IItem> Inventory { get; }

        IEnumerable<IItem> Equipments { get; }

        IEnumerable<KnownAbility> KnownAbilities { get; } // race abilities + class abilities + other if any (if an ability is found in multiple scope, the lowest level, cost, ... will be choosen)

        void Send(string msg, params string[] args);
    }
}
