using Mud.Domain;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Affects.Character;

[Affect("CharacterFlagsAffect", typeof(CharacterFlagsAffectData))]
public class CharacterFlagsAffect : FlagsAffectBase<ICharacterFlags, ICharacterFlagValues>, ICharacterFlagsAffect
{
    protected override string Target => "Flags";

    public CharacterFlagsAffect()
    {
    }

    public CharacterFlagsAffect(CharacterFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = data.Modifier;
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
            Modifier = Modifier
        };
    }
}
