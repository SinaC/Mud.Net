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

        IContainer ContainedInto { get; }
        bool ChangeContainer(IContainer container);

        ICharacter EquippedBy { get; }
        bool ChangeEquippedBy(ICharacter character, bool recompute);
    }
}
