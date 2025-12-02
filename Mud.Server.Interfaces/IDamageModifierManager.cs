using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces
{
    public interface IDamageModifierManager
    {
        ResistanceLevels ModifyDamage(ICharacter source, ICharacter victim, SchoolTypes damageType, ref int damage);
        ResistanceLevels ModifyDamage(ICharacter victim, SchoolTypes damageType, ref int damage);
        ResistanceLevels CheckResistance(ICharacter victim, SchoolTypes damageType);
    }
}
