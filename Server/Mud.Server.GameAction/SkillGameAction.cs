using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public abstract class SkillGameAction : CharacterGameActionBase<ICharacter, ISkillGameActionInfo>
{
}
