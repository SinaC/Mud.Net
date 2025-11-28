using Mud.Domain;

namespace Mud.Server.Interfaces.Affect.Character;

public interface ICharacterSizeAffect : ICharacterAffect
{
    Sizes Value { get; set; }
}
