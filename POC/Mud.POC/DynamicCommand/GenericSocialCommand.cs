using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC.DynamicCommand
{
    // no attribute because this command will be used to generate a command for every social
    [GenericCommand]
    public class GenericSocialCommand : CharacterGameAction
    {
        public override void Execute(IActionInput actionInput)
        {
            // TODO: extract social and parameters
        }
    }
}
