using Mud.POC.Abilities2.Domain;
using System;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IItem : IEntity
    {
        int Level { get; }
        int Weight { get; }

        ItemFlags ItemFlags { get; }
        void RemoveBaseItemFlags(ItemFlags itemFlags);

        int DecayPulseLeft { get; }
        void SetTimer(TimeSpan duration);

        void Disenchant();
        void IncreaseLevel();

        bool ChangeContainer(IContainer container);

        bool ChangeEquippedBy(ICharacter character, bool recompute);
    }
}
