using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class CharacterGameActionInfo : ActorGameActionInfo, ICharacterGameActionInfo
{
    public Positions MinPosition { get; }
    public bool NotInCombat { get; }

    public CharacterGameActionInfo(Type commandExecutionType, CharacterCommandAttribute characterCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes, HelpAttribute? helpAttribute)
        : base(commandExecutionType, characterCommandAttribute, syntaxAttribute, aliasAttributes, helpAttribute)
    {
        MinPosition = characterCommandAttribute.MinPosition;
        NotInCombat = characterCommandAttribute.NotInCombat;
    }
}
