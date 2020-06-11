using Mud.Domain;

namespace Mud.Server.Interfaces.Aura
{
    public interface ICharacterSexAffect : ICharacterAffect
    {
        Sex Value { get; set; }
    }
}
