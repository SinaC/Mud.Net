using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class AdminGameActionInfo : PlayerGameActionInfo, IAdminGameActionInfo
{
    public AdminLevels MinLevel { get; }

    public AdminGameActionInfo(Type commandExecutionType, AdminCommandAttribute adminCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
        : base(commandExecutionType, adminCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
        MinLevel = adminCommandAttribute.MinLevel;
    }
}
