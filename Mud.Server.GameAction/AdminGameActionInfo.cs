using Mud.Domain;
using Mud.Server.Interfaces.GameAction;
using System;

namespace Mud.Server.GameAction
{
    public class AdminGameActionInfo : PlayerGameActionInfo, IAdminGameActionInfo
    {
        public AdminLevels MinLevel { get; }

        public AdminGameActionInfo(Type commandExecutionType, AdminCommandAttribute adminCommandAttribute, SyntaxAttribute syntaxAttribute)
            : base(commandExecutionType, adminCommandAttribute, syntaxAttribute)
        {
            MinLevel = adminCommandAttribute.MinLevel;
        }
    }
}
