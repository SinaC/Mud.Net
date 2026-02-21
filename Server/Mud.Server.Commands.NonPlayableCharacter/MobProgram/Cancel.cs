using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpcancel", "MobProgram", Hidden = true)]
[Syntax("mob cancel")]
[Help("Reverse of 'mob delay', deactivates the timer.")]
public class Cancel : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [];

    public override void Execute(IActionInput actionInput)
    {
        Actor.ResetMobProgramDelay();
    }
}
