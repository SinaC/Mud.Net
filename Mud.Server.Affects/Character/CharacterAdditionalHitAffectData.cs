using Mud.Domain.Serialization;
using Mud.Domain.SerializationData;

namespace Mud.Server.Affects.Character
{
    [JsonBaseType(typeof(AffectDataBase))]
    public class CharacterAdditionalHitAffectData : AffectDataBase
    {
        public required int AdditionalHitCount { get; set; } = 1;
    }
}
