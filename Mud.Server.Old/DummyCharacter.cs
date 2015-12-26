using Mud.Server.Old.Commands;

namespace Mud.Server.Old
{
    public class DummyCharacter : EntityBase, ICharacter
    {
        public DummyCharacter(ICommandProcessor processor) 
            : base(processor)
        {
        }
    }
}
