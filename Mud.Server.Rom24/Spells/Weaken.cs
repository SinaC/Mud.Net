using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff)]
[AbilityCharacterWearOffMessage("You feel stronger.")]
[AbilityDispellable("{0:N} looks stronger.")]
public class Weaken : CharacterDebuffSpellBase
{
    private const string SpellName = "Weaken";

    public Weaken(IRandomManager randomManager, IAuraManager auraManager)
        : base(randomManager, auraManager)
    {
    }

    protected override SchoolTypes DebuffType => SchoolTypes.Other;
    protected override string VictimAffectMessage => "You feel your strength slip away.";
    protected override string RoomAffectMessage => "{0:N} looks tired and weak.";
    protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
        => (Level, TimeSpan.FromMinutes(Level / 2),
        new IAffect[]
        {
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -Level/5, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags("Weaken"), Operator = AffectOperators.Or }
        });

    protected override bool CanAffect => base.CanAffect && !Victim.CharacterFlags.IsSet("Weaken");
}
