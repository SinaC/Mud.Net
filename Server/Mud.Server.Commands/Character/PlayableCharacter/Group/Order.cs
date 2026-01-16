using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Group;

[PlayableCharacterCommand("order", "Pet"), MinPosition(Positions.Resting)]
[Syntax(
        "[cmd] <pet|charmie> command",
        "[cmd] all command")]
[Help(
@"[cmd] orders one or all of your charmed followers (including pets) to
perform any command.  The command may have arguments.  You are responsible
for the actions of your followers, and others who attack your followers
will incur the same penalty as if they attacked you directly.

Most charmed creatures lose their aggresive nature (while charmed).

If your charmed creature engages in combat, that will break the charm.")]
public class Order : PlayableCharacterGameAction
{
    private ICommandParser CommandParser { get; }

    public Order(ICommandParser commandParser)
    {
        CommandParser = commandParser;
    }

    protected INonPlayableCharacter[] Whom { get; set; } = default!;
    protected string CommandLine { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length < 2)
            return "Order whom to do what?";

        // Select target(s)
        if (actionInput.Parameters[0].IsAll)
            Whom = Actor.Room.NonPlayableCharacters.Where(x => x.Master == Actor && x.CharacterFlags.IsSet("Charm")).ToArray();
        else
        {
            var target = FindHelpers.FindByName(Actor.Room.NonPlayableCharacters.Where(Actor.CanSee), actionInput.Parameters[0]);
            if (target == null)
                return StringHelpers.CharacterNotFound;

            if (target.Master != Actor || !target.CharacterFlags.IsSet("Charm"))
                return "Do it yourself!";
            Whom = [target];
        }

        if (Whom.Length == 0)
            return "You don't have followers here.";

        CommandLine = CommandParser.JoinParameters(actionInput.Parameters.Skip(1));
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (var target in Whom)
        {
            Actor.Act(ActOptions.ToCharacter, "You order {0:N} to '{1}'.", target, CommandLine);
            target.Order(CommandLine);
        }

        Actor.SetGlobalCooldown(Pulse.PulseViolence);
        Actor.Send("Ok.");
    }
}
