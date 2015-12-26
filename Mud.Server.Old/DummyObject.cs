using Mud.Server.Old.Commands;

namespace Mud.Server.Old
{
    public class DummyObject : EntityBase, IObject
    {
        public DummyObject(ICommandProcessor processor) 
            : base(processor)
        {
        }
    }
}
