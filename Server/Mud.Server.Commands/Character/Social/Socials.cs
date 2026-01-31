using Mud.Server.Common.Attributes;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Social;
using System.Text;

namespace Mud.Server.Commands.Character.Social;

[CharacterCommand("socials", "Misc")]
[Help(
@"Display list of available socials")]
public class Socials : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [];

    private ISocialManager SocialManager { get; }

    public Socials(ISocialManager socialManager)
    {
        SocialManager = socialManager;
    }

    public override void Execute(IActionInput actionInput)
    {
        var sb = new StringBuilder(1024);
        var col = 0;
        foreach (var social in SocialManager.SocialDefinitionByName.Keys.OrderBy(x => x))
        {
            sb.AppendFormat("{0,14}", social);
            if (++col % 5 == 0)
                sb.AppendLine();
        }
        Actor.Page(sb);
    }
}
