using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Player;
using Mud.Settings.Interfaces;

namespace Mud.Server.Admin.Administration;

[AdminCommand("xpbonus", "Admin")]
[Syntax("[cmd] <player name> <experience>")]
public class XpBonus : AdminGameAction
{
    private IPlayerManager PlayerManager { get; }
    private IServerPlayerCommand ServerPlayerCommand { get; }
    private ISettings Settings { get; }
    private IWiznet Wiznet { get; }

    public XpBonus(IPlayerManager playerManager, IServerPlayerCommand serverPlayerCommand , ISettings settings, IWiznet wiznet)
    {
        PlayerManager = playerManager;
        ServerPlayerCommand = serverPlayerCommand;
        Settings = settings;
        Wiznet = wiznet;
    }

    protected IPlayableCharacter Whom { get; set; } = default!;
    protected int Experience { get; set; }

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length < 2)
            return BuildCommandSyntax();

        if (!actionInput.Parameters[1].IsNumber)
            return BuildCommandSyntax();

        Whom = FindHelpers.FindByName(PlayerManager.Players.Where(x => x.Impersonating != null).Select(x => x.Impersonating), actionInput.Parameters[0])!;
        if (Whom == null)
            return "That impersonated player is not here.";

        Experience = actionInput.Parameters[1].AsNumber;
        if (Experience < 1)
            return "Experience must be greater than 1.";

        if (Whom.Level >= Settings.MaxLevel)
            return $"{Whom.DisplayName} is already at max level.";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        Wiznet.Wiznet($"{Actor.DisplayName} give experience [{Experience}] to {Whom.DebugName}.", WiznetFlags.Help);

        Whom.Send("You have received an experience boost.");
        Whom.GainExperience(Experience);

        //
        ServerPlayerCommand.Save(Whom.ImpersonatedBy!);
    }
}
