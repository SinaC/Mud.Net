using Mud.Domain;
using Mud.Server.Interfaces.GameAction;
using System;
using System.Linq;
using System.Reflection;

namespace Mud.Server.GameAction
{
    public class CharacterGameActionInfo : GameActionInfo, ICharacterGameActionInfo
    {
        public Positions MinPosition { get; }

        public CharacterGameActionInfo(Type commandExecutionType, CharacterCommandAttribute characterCommandAttribute, SyntaxAttribute syntaxAttribute)
            : base(commandExecutionType, characterCommandAttribute, syntaxAttribute)
        {
            MinPosition = characterCommandAttribute.MinPosition;
        }
    }
}
