using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Blueprints;

namespace Mud.Server
{
    public interface IObject : IEntity
    {
        ObjectBlueprint Blueprint { get; }

        // TODO
    }
}
