using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
public class BurningHands : DamageTableSpellBase
{
    private const string SpellName = "Burning Hands";

    public BurningHands(IRandomManager randomManager)
        : base(randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Fire;
    protected override string DamageNoun => "burning hands";
    protected override int[] Table =>
    [
         0,
         0,  0,  0,  0, 14, 17, 20, 23, 26, 29,
        29, 29, 30, 30, 31, 31, 32, 32, 33, 33,
        34, 34, 35, 35, 36, 36, 37, 37, 38, 38,
        39, 39, 40, 40, 41, 41, 42, 42, 43, 43,
        44, 44, 45, 45, 46, 46, 47, 47, 48, 48
    ];

}
