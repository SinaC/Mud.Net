using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Affects.Character;

[Affect("CharacterBodyPartsAffect", typeof(CharacterBodyPartsAffectData))]
public class CharacterBodyPartsAffect : FlagsAffectBase<IBodyParts>, ICharacterBodyPartsAffect
{
    protected override string Target => "Body parts";

    public void Initialize(CharacterBodyPartsAffectData data)
    {
        Operator = data.Operator;
        Modifier = new BodyParts(data.Modifier);
    }

    public void Apply(ICharacter character)
    {
        character.ApplyAffect(this);
    }

    public override AffectDataBase MapAffectData()
    {
        return new CharacterBodyPartsAffectData
        {
            Operator = Operator,
            Modifier = Modifier.Serialize()
        };
    }
}
