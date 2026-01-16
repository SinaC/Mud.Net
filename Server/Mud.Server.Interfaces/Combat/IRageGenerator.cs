using Mud.Domain;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Interfaces.Combat;

public interface IRageGenerator
{
    public void GenerateRageFromIncomingDamage(ICharacter character, int damage, DamageSources damageSource);
    public void GenerateRageFromOutgoingDamage(ICharacter character, int damage, DamageSources damageSource);
}
