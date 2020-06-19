using Mud.Server.Interfaces.GameAction;
using System;
using System.Collections.Generic;

namespace Mud.Server.GameAction
{
    public class PlayableCharacterGameActionInfo : CharacterGameActionInfo, IPlayableCharacterGameActionInfo
    {
        public PlayableCharacterGameActionInfo(Type commandExecutionType, PlayableCharacterCommandAttribute playableCharacterCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes)
            : base(commandExecutionType, playableCharacterCommandAttribute, syntaxAttribute, aliasAttributes)
        {
        }
    }
}
