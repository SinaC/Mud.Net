using Mud.Domain.SerializationData;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Affects.Character;

[Affect("CharacterShieldFlagsAffect", typeof(CharacterShieldFlagsAffectData))]
public class CharacterShieldFlagsAffect : FlagsAffectBase<IShieldFlags, IShieldFlagValues>, ICharacterShieldFlagsAffect
{
    protected override string Target => "Shields";

    public CharacterShieldFlagsAffect()
    {
    }

    public void Initialize(CharacterShieldFlagsAffectData data)
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
        return new CharacterShieldFlagsAffectData
        {
            Operator = Operator,
            Modifier = Modifier
        };
    }
}
