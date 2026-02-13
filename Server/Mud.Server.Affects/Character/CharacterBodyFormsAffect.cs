using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Affects.Character;

[Affect("CharacterBodyFormsAffect", typeof(CharacterBodyFormsAffectData))]
public class CharacterBodyFormsAffect : FlagsAffectBase<IBodyForms>, ICharacterBodyFormsAffect
{
    protected override string Target => "Body forms";

    public void Initialize(CharacterBodyFormsAffectData data)
    {
        Operator = data.Operator;
        Modifier = new BodyForms(data.Modifier);
    }

    public void Apply(ICharacter character)
    {
        character.ApplyAffect(this);
    }

    public override AffectDataBase MapAffectData()
    {
        return new CharacterBodyFormsAffectData
        {
            Operator = Operator,
            Modifier = Modifier.Serialize()
        };
    }
}
