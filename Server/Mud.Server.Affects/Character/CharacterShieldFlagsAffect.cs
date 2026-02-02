using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Affects.Character;

[Affect("CharacterShieldFlagsAffect", typeof(CharacterShieldFlagsAffectData))]
public class CharacterShieldFlagsAffect : FlagsAffectBase<IShieldFlags>, ICharacterShieldFlagsAffect
{
    protected override string Target => "Shields";

    public void Initialize(CharacterShieldFlagsAffectData data)
    {
        Operator = data.Operator;
        Modifier = new ShieldFlags(data.Modifier);
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
