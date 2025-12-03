using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Random;

namespace Mud.Server.Tests.Mocking;

[Spell(SpellName, AbilityEffects.Damage)]
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells inflict damage on the victim.  The higher-level spells do
more damage.")]
public class Rom24AcidBlast : DamageSpellBase
{
    private const string SpellName = "Acid Blast";

    public Rom24AcidBlast(ILogger<Rom24AcidBlast> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Acid;
    protected override int DamageValue => RandomManager.Dice(Level, 12);
    protected override string DamageNoun => "acid blast";
}
