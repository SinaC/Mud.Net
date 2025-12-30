using Mud.Common.Attributes;
using Mud.Domain.SerializationData;

namespace Mud.Server.POC.Affects;

[JsonBaseType(typeof(AffectDataBase))]
public class NextHitDamageModifierAffectData : AffectDataBase
{
    public int Modifier { get; set; } = 0;
}