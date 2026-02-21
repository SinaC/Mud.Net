using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpforget", "MobProgram", Hidden = true)]
[Help("Reverse of 'mob remember'")]
[Syntax("mob forget")]
public class Forget : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    public override void Execute(IActionInput actionInput)
    {
        Actor.SetMobProgramTarget(null);
    }
}
