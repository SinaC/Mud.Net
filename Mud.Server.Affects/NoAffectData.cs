using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Affects;

[JsonPolymorphism(typeof(AffectDataBase), "noData")]
public sealed class NoAffectData : AffectDataBase
{
    public required string AffectName { get; set; }
}
