using Mud.Common;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Common.Extensions;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;
using System.Text;

namespace Mud.Server.Affects.Character;

[Affect("CharacterResourceAffect", typeof(CharacterResourceAffectData))]
public class CharacterResourceAffect : ICharacterResourceAffect
{
    public ResourceKinds Location { get; set; }
    public AffectOperators Operator { get; set; } // Or and Nor cannot be used
    public int Modifier { get; set; }

    protected string Target => Location.ToString();

    public void Initialize(CharacterResourceAffectData data)
    {
        Location = data.Location;
        Operator = data.Operator;
        Modifier = data.Modifier;
    }

    public void Append(StringBuilder sb)
    {
        sb.AppendFormat("%c%modifies %y%{0} %c%{1} %y%{2}%x%", Target.ToPascalCase(), Operator.PrettyPrint(), Modifier);
    }

    public void Apply(ICharacter character)
    {
        character.ApplyAffect(this);
    }

    public AffectDataBase MapAffectData()
    {
        return new CharacterResourceAffectData
        {
            Location = Location,
            Operator = Operator,
            Modifier = Modifier
        };
    }
}
