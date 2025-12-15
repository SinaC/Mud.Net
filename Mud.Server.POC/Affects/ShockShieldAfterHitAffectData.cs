using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.POC.Affects;

[JsonPolymorphism(typeof(AffectDataBase), "shockshield")]
public class ShockShieldAfterHitAffectData : AffectDataBase
{
}
