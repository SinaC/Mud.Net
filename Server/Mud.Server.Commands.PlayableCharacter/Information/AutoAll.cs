using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.PlayableCharacter.Information;

[PlayableCharacterCommand("autoall", "Information")]
[Syntax("[cmd]")]
[Help(
@"Active every 'auto'")]
public class AutoAll : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [];

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
