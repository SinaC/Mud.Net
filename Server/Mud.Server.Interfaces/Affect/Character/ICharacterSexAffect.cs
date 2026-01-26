using Mud.Domain;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterSexAffect : ICharacterAffect
{
    Sex Value { get; }
}
