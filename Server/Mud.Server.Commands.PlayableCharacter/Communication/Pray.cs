using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.PlayableCharacter.Communication;

[PlayableCharacterCommand("pray", "Communication")]
[Syntax("[cmd] <msg>")]
[Help(@"This commands allows players to send a message to admins.")]
public class Pray : PlayableCharacterGameAction
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new RequiresAtLeastOneArgument { Message = "Pay what ?" }];

    private IParser Parser { get; }
    private IAdminManager AdminManager { get; }

    public Pray(IParser parser, IAdminManager adminManager)
    {
        Parser = parser;
        AdminManager = adminManager;
    }
 
    private string What { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        What = Parser.JoinParameters(actionInput.Parameters);
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        string phrase = $"%g%{Actor.DisplayName} has prayed '%x%{What}%g%'%x%";
        foreach (IAdmin admin in AdminManager.Admins)
            admin.Send(phrase);
    }
}
