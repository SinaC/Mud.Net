using Mud.POC.Abilities2.Domain;
using System;

namespace Mud.POC.Abilities2.Interfaces
{
    public interface IItem : IEntity
    {
        int Level { get; }

        ItemFlags ItemFlags { get; }
        bool RemoveBaseItemFlags(ItemFlags flags);

        int DecayPulseLeft { get; }
        void SetTimer(TimeSpan duration);
    }
}
