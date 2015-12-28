using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Blueprints;

namespace Mud.Server
{
    public interface IExit
    {
        ExitBlueprint Blueprint { get; }

        string Name { get; } // should be equal to first word of keywords in blueprint
        string Description { get; }
        // TODO: key blueprint id or key blueprint
        // TODO: flags
        IRoom Destination { get; }
    }
}
