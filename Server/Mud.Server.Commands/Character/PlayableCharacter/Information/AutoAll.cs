using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Flags;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Information;

[PlayableCharacterCommand("autoall", "Information")]
[Syntax("[cmd]")]
[Help(
@"Active every 'auto'")]
public class AutoAll : PlayableCharacterGameAction
{
    private IFlagsManager FlagsManager { get; }

    public AutoAll(IFlagsManager flagsManager)
    {
        FlagsManager = flagsManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        var availableFlags = FlagsManager.AvailableValues<IAutoFlags>();

        foreach (var autoFlag in FlagsManager.AvailableValues<IAutoFlags>().OrderBy(x => x))
            Actor.AddAutoFlags(new AutoFlags(autoFlag));
        Actor.Send("Ok.");
    }
}
