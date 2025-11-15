using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("You feel less armored.")]
public class Armor : CharacterBuffSpellBase
{
    private const string SpellName = "Armor";

    public Armor(IRandomManager randomManager, IAuraManager auraManager) 
        : base(randomManager, auraManager)
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
