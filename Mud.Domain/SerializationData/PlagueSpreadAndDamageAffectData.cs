using Mud.Domain.Serialization;

namespace Mud.Domain.SerializationData;

[JsonPolymorphism(typeof(AffectDataBase), "plague")]
public class PlagueSpreadAndDamageAffectData : AffectDataBase
{
}
