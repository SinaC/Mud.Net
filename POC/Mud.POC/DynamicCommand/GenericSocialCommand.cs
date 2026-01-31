using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.POC.DynamicCommand
{
    // no attribute because this command will be used to generate a command for every social
    [GenericCommand]
    public class GenericSocialCommand : CharacterGameAction
    {
        protected override IGuard<ICharacter>[] Guards => [];

        public override void Execute(IActionInput actionInput)
        {
            // TODO: extract social and parameters
        }
    }
}
