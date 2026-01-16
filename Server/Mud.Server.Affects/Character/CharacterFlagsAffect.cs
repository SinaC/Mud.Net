using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Affects.Character;

[Affect("CharacterFlagsAffect", typeof(CharacterFlagsAffectData))]
public class CharacterFlagsAffect : FlagsAffectBase<ICharacterFlags>, ICharacterFlagsAffect
{
    protected override string Target => "Flags";

    public void Initialize(CharacterFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = new CharacterFlags(data.Modifier);
    }

    public void Apply(ICharacter character)
    {
        character.ApplyAffect(this);
    }

    public override AffectDataBase MapAffectData()
    {
        return new CharacterFlagsAffectData
        {
            Operator = Operator,
            Modifier = Modifier.Serialize()
        };
    }
}
