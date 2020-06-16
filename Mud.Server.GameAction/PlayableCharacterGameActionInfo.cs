using Mud.Server.Input;
using Mud.Server.Interfaces.GameAction;
using System;

namespace Mud.Server.GameAction
{
    public class PlayableCharacterGameActionInfo : CharacterGameActionInfo, IPlayableCharacterGameActionInfo
    {
        public PlayableCharacterGameActionInfo(Type commandExecutionType, PlayableCharacterCommandAttribute playableCharacterCommandAttribute, SyntaxAttribute syntaxAttribute)
            : base(commandExecutionType, playableCharacterCommandAttribute, syntaxAttribute)
        {
        }
    }
}
