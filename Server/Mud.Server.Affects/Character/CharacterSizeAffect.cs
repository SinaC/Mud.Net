using Mud.Common;
using Mud.Domain;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Affects.Character;

[Affect("CharacterSizeAffect", typeof(CharacterSizeAffectData))]
public class CharacterSizeAffect : ICharacterSizeAffect
{
    public Sizes Value { get; set; }

    public void Initialize(CharacterSizeAffectData data)
    {
        Value = data.Value;
    }

    public void Append(StringBuilder sb)
    {
        sb.AppendFormat("%c%modifies %y%Size %c%by setting to %y%{0}%x%", Value.ToString().ToPascalCase());
    }

    public void Apply(ICharacter character)
    {
        character.ApplyAffect(this);
    }

    public AffectDataBase MapAffectData()
    {
        return new CharacterSizeAffectData
        {
            Value = Value
        };
    }
}
