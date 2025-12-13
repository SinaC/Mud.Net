using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Affects.Character;

[Affect("CharacterFlagsAffect", typeof(CharacterFlagsAffectData))]
public class CharacterFlagsAffect : FlagsAffectBase<ICharacterFlags, ICharacterFlagValues>, ICharacterFlagsAffect
{
    protected override string Target => "Flags";

    private IFlagFactory<ICharacterFlags, ICharacterFlagValues> Factory { get; }

    public CharacterFlagsAffect(IFlagFactory<ICharacterFlags, ICharacterFlagValues> factory)
    {
        Factory = factory;
    }

    public void Initialize(CharacterFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = Factory.CreateInstance(data.Modifier);
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
