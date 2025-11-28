using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("You feel less armored.")]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell decreases (improves) the armor class of the target character
by 20 points.")]
public class Armor : CharacterBuffSpellBase
{
    private const string SpellName = "Armor";

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
