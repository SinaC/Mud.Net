using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage)]
[Syntax("cast [spell] <character>")]
[Help(
@"These spells inflict damage on the victim.  The higher-level spells do
more damage.")]
[OneLineHelp("the most deadly harmful spell")]
public class Harm : OffensiveSpellBase
{
    private const string SpellName = "Harm";

    public Harm(ILogger<Harm> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override void Invoke()
    {
        int damage = Math.Max(20, Victim[ResourceKinds.HitPoints] - RandomManager.Dice(1, 4));
        if (Victim.SavesSpell(Level, SchoolTypes.Harm))
            damage = Math.Min(50, damage / 2);
        damage = Math.Min(100, damage);
        Victim.AbilityDamage(Caster, damage, SchoolTypes.Harm, "harm spell", true);
    }
}
