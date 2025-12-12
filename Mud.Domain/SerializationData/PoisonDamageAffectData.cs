using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(AffectDataBase), "poison")]
public class PoisonDamageAffectData : AffectDataBase
{
}
