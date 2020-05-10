using System.Collections.Generic;
using Mud.Domain;
using Mud.Server.Blueprints.Room;

namespace Mud.Server
{
    public interface IExit
    {
        ExitBlueprint Blueprint { get; }

        string Name { get; } // should be equal to first word of keywords in blueprint
        IEnumerable<string> Keywords { get; }
        string Description { get; }
        IRoom Destination { get; }
        ExitFlags CurrentFlags { get; }

        bool IsDoor { get; }
        bool IsClosed { get; }
        bool IsLocked { get; }
        bool IsHidden { get; }

        void Open();
        void Close();
        void Unlock();
        void Lock();

        void OnRemoved();
    }
}
