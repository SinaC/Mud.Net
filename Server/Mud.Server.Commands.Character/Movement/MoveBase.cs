using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Movement
{
    public abstract class MoveBase : CharacterGameAction
    {
        protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new CannotBeInCombat()];

        protected abstract ExitDirections Direction { get; }

        public override void Execute(IActionInput actionInput)
        {
            Actor.Move(Direction, false, true);
        }
    }
}
