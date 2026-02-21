using Mud.Domain;
using Mud.Random;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpflee", "MobProgram", Hidden = true)]
[Help("Forces the mobile to flee.")]
[Syntax("mob flee")]
public class Flee : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new MustBeInCombat()];

    private IRandomManager RandomManager { get; }

    public Flee(IRandomManager randomManager)
    {
        RandomManager = randomManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        var wasInRoom = Actor.Room;
        // Try 10 times to find an exit
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var randomExit = RandomManager.Random<ExitDirections>() ?? ExitDirections.North;
            var exit = Actor.Room[randomExit];
            var destination = exit?.Destination;
            if (destination != null && exit?.IsClosed == false
                && Actor.IsAllowedToEnterTo(destination))
            {
                // Try to move without checking if in combat or not
                Actor.Move(randomExit, false, false); // followers will not follow
                if (Actor.Room != wasInRoom) // successful only if effectively moved away
                    break;
            }
        }
    }
}
