using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;
namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 18)]
[AbilityCharacterWearOffMessage("Your force shield shimmers then fades away.")]
[AbilityDispellable("The shield protecting {0:n} vanishes.")]
[Syntax("cast [spell]")]
[Help(
@"These spells protect the caster by decreasing (improving) the caster's armor
class. It provides 20 points of armor.")]
[OneLineHelp("puts a shimmering shield between you and your enemies")]
public class Shield : CharacterBuffSpellBase
{
    private const string SpellName = "Shield";

    public Shield(ILogger<Shield> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager, auraManager)
    {
    }

    protected override string SelfAlreadyAffectedMessage => "You are already shielded from harm.";
    protected override string NotSelfAlreadyAffectedMessage => "{0:N} is already protected by a shield.";
    protected override string VictimAffectMessage => "You are surrounded by a force shield.";
    protected override string CasterAffectMessage => "{0:N} {0:b} surrounded by a force shield.";
    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
        => (Level, TimeSpan.FromMinutes(8 + Level),
        new IAffect[]
        {
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -20, Operator = AffectOperators.Add }
        });
}
