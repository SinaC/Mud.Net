using Mud.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

public abstract class AutoBase : PlayableCharacterGameAction
{
    protected abstract string What { get; }
    protected abstract string RemovedMessage { get; }
    protected abstract string AddedMessage { get; }

    public override void Execute(IActionInput actionInput)
    {
        if (Actor.AutoFlags.IsSet(What))
        {
            Actor.RemoveAutoFlags(new AutoFlags(What));
            Actor.Send(RemovedMessage);
        }
        else
        {
            Actor.AddAutoFlags(new AutoFlags(What));
            Actor.Send(AddedMessage);
        }
    }
}
