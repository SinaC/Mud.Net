using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;

namespace Mud.Server.POC.Affects;

[JsonBaseType(typeof(AffectDataBase))]
public class AbsorbDamageAffectData : AffectDataBase
{
    public int RemainingAbsorb { get; set; } = 0;
}
