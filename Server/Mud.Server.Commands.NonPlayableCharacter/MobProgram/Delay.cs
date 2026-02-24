using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.NonPlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.NonPlayableCharacter.MobProgram;

[NonPlayableCharacterCommand("mpdelay", "MobProgram", Hidden = true)]
[Syntax("mob delay [pulses]")]
[Help(
@"Sets a delay for MOBprogram execution. When the delay time expires,
the mobile is checked for a MObprogram with DELAY trigger, and if
one is found, it is executed. Delay is counted in PULSE_MOBILE")]
public class Delay : NonPlayableCharacterGameAction
{
    protected override IGuard<INonPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument()];

    private int PulseCount { get; set; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (!actionInput.Parameters[0].IsNumber)
            return "Delay must be an integer";
        var pulseCount = actionInput.Parameters[0].AsNumber;
        if (pulseCount <= 0)
            return "Delay must be strictly positive";
        PulseCount = pulseCount;

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Actor.SetMobProgramDelay(PulseCount);
    }
}
