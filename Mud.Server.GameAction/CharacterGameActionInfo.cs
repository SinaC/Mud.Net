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

        public new static ICharacterGameActionInfo Create(Type type) // TODO: replace with ctor when CommandExecutionInfo and CommandMethodInfo will be removed
        {
            CharacterCommandAttribute commandAttribute = type.GetCustomAttributes<CharacterCommandAttribute>().FirstOrDefault();
            SyntaxAttribute syntaxAttribute = type.GetCustomAttribute<SyntaxAttribute>() ?? DefaultSyntaxCommandAttribute;
            return new CharacterGameActionInfo(type, commandAttribute, syntaxAttribute);
        }
    }
}
