using System.Collections.Generic;
using Mud.Domain;

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

        int this[ResourceKinds resource] { get; }
        IEnumerable<ResourceKinds> CurrentResourceKinds { get; }

        int BaseAttributes(CharacterAttributes attribute);
        int CurrentAttributes(CharacterAttributes attribute);

        int GetMaxResource(ResourceKinds resource);
        void UpdateResource(ResourceKinds resource, int amount);

        void Send(string msg, params object[] args);
    }
}
