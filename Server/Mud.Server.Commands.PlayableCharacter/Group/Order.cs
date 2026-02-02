using Mud.Domain;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Common;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Group;

[PlayableCharacterCommand("order", "Pet")]
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
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Resting)];

    private IParser Parser { get; }

    public Order(IParser parser)
    {
        Parser = parser;
    }

    private INonPlayableCharacter[] Whom { get; set; } = default!;
    private string CommandLine { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
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

        CommandLine = Parser.JoinParameters(actionInput.Parameters.Skip(1));
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
