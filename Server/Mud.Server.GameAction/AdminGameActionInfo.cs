using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;

namespace Mud.Server.GameAction;

public class AdminGameActionInfo : PlayerGameActionInfo, IAdminGameActionInfo
{
    public IAdminGuard[] AdminGuards { get; }

    public AdminGameActionInfo(Type commandExecutionType, AdminCommandAttribute adminCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute, IEnumerable<IActorGuard> actorGuards, IEnumerable<IPlayerGuard> playerGuards, IEnumerable<IAdminGuard> adminGuards)
        : base(commandExecutionType, adminCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute, actorGuards, playerGuards)
    {
        AdminGuards = adminGuards.ToArray();
    }
}
