using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Affects.Character;

[Affect("CharacterShieldFlagsAffect", typeof(CharacterShieldFlagsAffectData))]
public class CharacterShieldFlagsAffect : FlagsAffectBase<IShieldFlags, IShieldFlagValues>, ICharacterShieldFlagsAffect
{
    private IFlagFactory<IShieldFlags, IShieldFlagValues> FlagFactory { get; }

    public CharacterShieldFlagsAffect(IFlagFactory<IShieldFlags, IShieldFlagValues> flagFactory)
    {
        FlagFactory = flagFactory;
    }

    protected override string Target => "Shields";

    public void Initialize(CharacterShieldFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = FlagFactory.CreateInstance(data.Modifier);
    }

    public void Apply(ICharacter character)
    {
        character.ApplyAffect(this);
    }

    public override AffectDataBase MapAffectData()
    {
        return new CharacterShieldFlagsAffectData
        {
            Operator = Operator,
            Modifier = Modifier.Serialize()
        };
    }
}
