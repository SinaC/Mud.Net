using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.SpellGuards;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("You feel less armored.")]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell decreases (improves) the armor class of the target character
by 20 points.")]
[OneLineHelp("provides the target with an extra layer of defense")]
public class Armor : CharacterBuffSpellBase
{
    private const string SpellName = "Armor";

    protected override ISpellGuard[] Guards => [new CannotBeInCombat()];

    public Armor(ILogger<Armor> logger, IRandomManager randomManager, IAuraManager auraManager) 
        : base(logger, randomManager, auraManager)
    {
    }

    protected override string SelfAlreadyAffectedMessage => "You are already armored.";
    protected override string NotSelfAlreadyAffectedMessage => "{0:N} {0:b} already armored.";
    protected override string VictimAffectMessage => "You feel someone protecting you.";
    protected override string CasterAffectMessage => "{0:N} is protected by your magic.";
    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
        => (Level, TimeSpan.FromMinutes(24),
         new IAffect[]
         { 
             new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -20, Operator = AffectOperators.Add }
         });
}
