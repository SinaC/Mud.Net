using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Server.Commands;

namespace Mud.Server
{
    public class DummyCharacter : EntityBase, ICharacter
    {
        public DummyCharacter(ICommandProcessor processor) 
            : base(processor)
        {
        }
    }
}
