using Mud.Server.Old.Commands;

namespace Mud.Server.Old
{
    public class DummyRoom : EntityBase, IRoom
    {
        public DummyRoom(ICommandProcessor processor) 
            : base(processor)
        {
        }
    }
}
