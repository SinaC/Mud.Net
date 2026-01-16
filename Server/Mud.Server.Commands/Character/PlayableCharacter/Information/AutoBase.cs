using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

public abstract class AutoBase : PlayableCharacterGameAction
{
    protected abstract AutoFlags What { get; }
    protected abstract string RemovedMessage { get; }
    protected abstract string AddedMessage { get; }

    public override void Execute(IActionInput actionInput)
    {
        if (Actor.AutoFlags.HasFlag(What))
        {
            Actor.RemoveAutoFlags(What);
            Actor.Send(RemovedMessage);
        }
        else
        {
            Actor.AddAutoFlags(What);
            Actor.Send(AddedMessage);
        }
    }
}
