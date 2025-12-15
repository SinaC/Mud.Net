using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Combat
{
    public interface IResistanceCalculator
    {
        ResistanceLevels CheckResistance(ICharacter victim, SchoolTypes damageType);
    }
}
