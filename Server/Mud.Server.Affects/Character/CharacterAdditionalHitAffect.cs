using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Affects.Character;

[Affect("AdditionalHit", typeof(CharacterAdditionalHitAffectData))]
public class CharacterAdditionalHitAffect : ICharacterAdditionalHitAffect
{
    public int AdditionalHitCount { get; set; }

    public void Initialize(CharacterAdditionalHitAffectData data)
    {
        AdditionalHitCount = data.AdditionalHitCount;
    }

    public bool IsAdditionalHitAvailable(ICharacter character, int hitCount)
        => !character.CharacterFlags.IsSet("Slow"); // no additional hit if affected by slow

    public void Append(StringBuilder sb)
    {
        if (AdditionalHitCount == 1)
            sb.Append("%c%gives %y%one %c%additional hit%x%");
        else
            sb.AppendFormat("%c%gives %y%{0} %c%additional hits%x%", AdditionalHitCount);
    }

    public AffectDataBase MapAffectData()
    {
        return new CharacterAdditionalHitAffectData { AdditionalHitCount = AdditionalHitCount };
    }
}
