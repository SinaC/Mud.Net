﻿using Mud.Server.Blueprints;
using Mud.Server.Blueprints.Room;

namespace Mud.Server
{
    public interface IExit
    {
        ExitBlueprint Blueprint { get; }

        string Name { get; } // should be equal to first word of keywords in blueprint
        string Keywords { get; }
        string Description { get; }
        // TODO: key blueprint id or key blueprint
        // TODO: flags
        IRoom Destination { get; }

        void OnRemoved();
    }
}
