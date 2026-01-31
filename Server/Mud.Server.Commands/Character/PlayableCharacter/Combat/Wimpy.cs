using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.Commands.Character.PlayableCharacter.Combat;

[PlayableCharacterCommand("wimpy", "Combat")]
[Syntax(
    "[cmd]",
    "[cmd] <number>")]
[Help(
@"[cmd] sets your wimpy value.  When your character takes damage that reduces
your hit points below your wimpy value, you will automatically attempt to flee.
You will only flee if your character is not in a wait state -- i.e. has not
been using combat commands like cast, trip and bash, and has not been
tripped or bash by an enemy.

[cmd] with no argument sets your wimpy value to 20% of your maximum hit points.

Some monsters are wimpy.")]
public class Wimpy : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing)];

    private int Number { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            Number = Actor.MaxResource(ResourceKinds.HitPoints) / 5;
        else if (!actionInput.Parameters[0].IsNumber)
            return BuildCommandSyntax();
        else
            Number = actionInput.Parameters[0].AsNumber;
        if (Number < 0)
            return "Your courage exceeds your wisdom.";
        else if (Number > Actor.MaxResource(ResourceKinds.HitPoints) / 2)
            return "Such cowardice ill becomes you.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.SetWimpy(Number);
        Actor.Send("Wimpy set to {0} hit points.", Number);
    }
}
