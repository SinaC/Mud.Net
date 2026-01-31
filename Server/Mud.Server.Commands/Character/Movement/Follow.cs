using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Character.Movement;

[CharacterCommand("follow", "Group", "Movement")]
[Syntax(
       "[cmd]",
       "[cmd] <character>")]
[Help(
@"FOLLOW starts you following another character.  To stop following anyone else,
just FOLLOW <yourself>.")]
public class Follow : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [];

    private bool DisplayLeader { get; set; }
    private ICharacter Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;


        if (actionInput.Parameters.Length == 0)
        {
            if (Actor.Leader == null)
                return "You are not following anyone.";
            DisplayLeader = true;
            return null;
        }

        // search target
        Whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        // follow ourself -> cancel follow
        if (Whom == Actor)
        {
            if (Actor.Leader == null)
                return "You already follow yourself.";
            return null;
        }

        // check cycle
        var next = Whom.Leader;
        while (next != null)
        {
            if (next == Actor)
                return Actor.ActPhrase("You can't follow {0:N}.", Whom);
            next = next.Leader;
        }

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (DisplayLeader)
        {
            Actor.Act(ActOptions.ToCharacter, "You are following {0:N}.", Actor.Leader!);
            return;
        }
        if (Whom == Actor)
        {
            Actor.Leader?.RemoveFollower(Actor);
            return;
        }

        Whom.Leader?.RemoveFollower(Actor);
        Whom.AddFollower(Actor);
    }
}
