using Mud.Domain;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;

namespace Mud.Server.GameAction
{
    public class AdminGameActionInfo : PlayerGameActionInfo, IAdminGameActionInfo
    {
        public AdminLevels MinLevel { get; }

        public AdminGameActionInfo(Type commandExecutionType, AdminCommandAttribute adminCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes)
            : base(commandExecutionType, adminCommandAttribute, syntaxAttribute, aliasAttributes)
        {
            MinLevel = adminCommandAttribute.MinLevel;
        }
    }
}
