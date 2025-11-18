using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Communication;

[PlayableCharacterCommand("pray", "Communication")]
[Syntax("[cmd] <msg>")]
[Help(@"This commands allows players to send a message to admins.")]
public class Pray : PlayableCharacterGameAction
{
    private IAdminManager AdminManager { get; }

    public Pray(IAdminManager adminManager)
    {
        AdminManager = adminManager;
    }
 
    protected string What { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            return "Pray what?";

        What = CommandHelpers.JoinParameters(actionInput.Parameters);
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        string phrase = $"%g%{Actor.DisplayName} has prayed '%x%{What}%g%'%x%";
        foreach (IAdmin admin in AdminManager.Admins)
            admin.Send(phrase);
    }
}
