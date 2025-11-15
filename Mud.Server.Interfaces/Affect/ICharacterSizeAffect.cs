using Mud.Domain;

namespace Mud.Server.Interfaces.Affect;

public interface ICharacterSizeAffect : ICharacterAffect
{
    Sizes Value { get; set; }
}
