using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Commands;

namespace Mud.Server
{
    public class DummyObject : EntityBase, IObject
    {
        public DummyObject(ICommandProcessor processor) 
            : base(processor)
        {
        }
    }
}
