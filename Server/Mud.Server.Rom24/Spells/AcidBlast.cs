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
[Syntax("cast [spell] <victim>")]
[Help(
@"These spells inflict damage on the victim.  The higher-level spells do
more damage.")]
[OneLineHelp("sends forth a stream of acid to eradicate your foes")]
public class AcidBlast : DamageSpellBase
{
    private const string SpellName = "Acid Blast";

    public AcidBlast(ILogger<AcidBlast> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected override SchoolTypes DamageType => SchoolTypes.Acid;
    protected override int DamageValue => RandomManager.Dice(Level, 12);
    protected override string DamageNoun => "acid blast";
}
