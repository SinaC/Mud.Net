using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.POC.Affects;

[JsonPolymorphism(typeof(AffectDataBase), "iceshield")]
public class IceShieldAfterHitAffectData : AffectDataBase
{
}
