using Mud.Domain;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public class CharacterGameActionInfo : GameActionInfo, ICharacterGameActionInfo
{
    public Positions MinPosition { get; }
    public bool NotInCombat { get; }

    public CharacterGameActionInfo(Type commandExecutionType, CharacterCommandAttribute characterCommandAttribute, SyntaxAttribute syntaxAttribute, IEnumerable<AliasAttribute> aliasAttributes)
        : base(commandExecutionType, characterCommandAttribute, syntaxAttribute, aliasAttributes)
    {
        MinPosition = characterCommandAttribute.MinPosition;
        NotInCombat = characterCommandAttribute.NotInCombat;
    }
}
