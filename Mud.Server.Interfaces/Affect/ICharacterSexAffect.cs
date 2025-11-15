using Mud.Domain;

namespace Mud.Server.Interfaces.Affect;

public interface ICharacterSexAffect : ICharacterAffect
{
    Sex Value { get; set; }
}
