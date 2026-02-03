using Mud.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Combat;

[CharacterCommand("flee", "Combat")]
[Help(
@"Once you start a fight, you can't just walk away from it.  If the fight
is not going well, you can attempt to [cmd], or another character can
RESCUE you.  (You can also RECALL, but this is less likely to work,
and costs more experience points, then fleeing).

If you lose your link during a fight, then your character will keep
fighting, and will attempt to RECALL from time to time.  Your chances
of making the recall are reduced, and you will lose much more experience.")]
public class Flee : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Standing), new MustBeInCombat()];

    public override void Execute(IActionInput actionInput)
    {
        Actor.Flee();
    }
}
