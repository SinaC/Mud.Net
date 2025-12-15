using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Rom24.Affects;

[JsonPolymorphism(typeof(AffectDataBase), "protectevil")]
public class ProtectionEvilDamageModifierAffectData : AffectDataBase
{
}
