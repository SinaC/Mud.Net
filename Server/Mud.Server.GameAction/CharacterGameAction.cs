using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public abstract class CharacterGameAction : CharacterGameActionBase<ICharacter, ICharacterGameActionInfo>
{
}
