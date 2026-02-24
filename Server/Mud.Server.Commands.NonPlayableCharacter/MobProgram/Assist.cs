using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpassist", "MobProgram", Hidden = true)]
[Syntax("mob assist [character]")]
[Help("Lets the mobile assist another mob or player")]
public class Assist : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new RequiresAtLeastOneArgument()];

    private ICharacter Whom { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0])!;
        if (Whom == null)
            return StringHelpers.CharacterNotFound;

        if (Whom == Actor)
            return "What about fleeing instead?";

        if (Actor.Fighting == Whom)
            return "Too late.";

        if (Whom.Fighting == null)
            return "Victim is not fighting right now.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // GCD
        Actor.SetGlobalCooldown(1);

        // Starts fight
        Actor.MultiHit(Whom);
    }
}
