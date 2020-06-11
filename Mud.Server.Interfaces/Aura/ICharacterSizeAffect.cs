using Mud.Domain;

namespace Mud.Server.Interfaces.Aura
{
    public interface ICharacterSizeAffect : ICharacterAffect
    {
        Sizes Value { get; set; }
    }
}
