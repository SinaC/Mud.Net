using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Affects.Character;

[Affect("CharacterIRVAffect", typeof(CharacterIRVAffectData))]
public class CharacterIRVAffect : FlagsAffectBase<IIRVFlags>, ICharacterIRVAffect
{
    protected override string Target => Location.ToString();

    public IRVAffectLocations Location { get; set; }

    public void Initialize(CharacterIRVAffectData data)
    {
        Location = data.Location;
        Operator = data.Operator;
        Modifier = new IRVFlags(data.Modifier);
    }

    public void Apply(ICharacter character)
    {
        character.ApplyAffect(this);
    }

    public override AffectDataBase MapAffectData()
    {
        return new CharacterIRVAffectData
        {
            Location = Location,
            Operator = Operator,
            Modifier = Modifier.Serialize()
        };
    }
}
