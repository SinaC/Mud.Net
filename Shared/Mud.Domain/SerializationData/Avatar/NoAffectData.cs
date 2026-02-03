using Mud.Common.Attributes;

namespace Mud.Domain.SerializationData.Avatar;

[JsonBaseType(typeof(AffectDataBase), "noData")]
public sealed class NoAffectData : AffectDataBase
{
    public required string AffectName { get; set; }
}
