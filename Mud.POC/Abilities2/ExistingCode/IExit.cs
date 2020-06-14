using System.Collections.Generic;
using Mud.POC.Abilities2.Domain;
using Mud.Server.Blueprints.Room;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IExit : ICloseable
    {
        ExitBlueprint Blueprint { get; }

        string Name { get; } // should be equal to first word of keywords in blueprint
        IEnumerable<string> Keywords { get; }
        string Description { get; }
        IRoom Destination { get; }
        ExitFlags ExitFlags { get; }

        bool IsDoor { get; }
        bool IsHidden { get; }

        void OnRemoved();
    }
}
