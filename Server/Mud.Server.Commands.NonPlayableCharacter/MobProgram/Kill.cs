using Mud.Domain;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpkill", "MobProgram", Hidden = true)]
[Help("Lets the mobile kill any player or mobile without murder")]
[Syntax("mob kill [victim]")]
public class Kill : NonPlayableCharacterGameAction
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
            return "You hit yourself. Ouch!";

        if (Whom is INonPlayableCharacter)
            return "You cannot kill another NPC";

        // no safe check
        // no kill stealing check

        if (Actor.CharacterFlags.IsSet("Charm") && Actor.Master == Whom)
            return Actor.ActPhrase("{0:N} is your beloved master.", Whom);

        if (Actor.Fighting != null)
            return "You do the best you can!";

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
