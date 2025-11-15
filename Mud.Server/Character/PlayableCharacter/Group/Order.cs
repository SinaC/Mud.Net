using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.PlayableCharacter.Group;

[PlayableCharacterCommand("order", "Pet")]
[Syntax(
        "[cmd] <pet|charmie> command",
        "[cmd] all command")]
public class Order : PlayableCharacterGameAction
{
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
            var target = FindHelpers.FindByName(Actor.Room.NonPlayableCharacters.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
            if (target == null)
                return StringHelpers.CharacterNotFound;

            if (target.Master != Actor || !target.CharacterFlags.IsSet("Charm"))
                return "Do it yourself!";
            Whom = [target];
        }

        if (Whom.Length == 0)
            return "You don't have followers here.";

        CommandLine = CommandHelpers.JoinParameters(actionInput.Parameters.Skip(1));
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        foreach (var target in Whom)
        {
            Actor.Act(ActOptions.ToCharacter, "You order {0:N} to '{1}'.", target, CommandLine);
            target.Order(CommandLine);
        }

        Actor.ImpersonatedBy?.SetGlobalCooldown(Pulse.PulseViolence);
        Actor.Send("Ok.");
    }
}
