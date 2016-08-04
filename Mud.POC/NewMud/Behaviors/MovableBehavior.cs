using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.POC.NewMud.Behaviors
{
    // Behavior applied to objects that can move or be moved.
    public class MovableBehavior : Behavior
    {
        public void Move(Thing destination, Thing goingVia /*, leavingMessage, enteringMessage*/)
        {
            Thing actor = Parent;
            Thing origin = actor.Parent;

            origin.PerformRemove(actor);
            destination.PerformAdd(actor);
        }
    }
}
