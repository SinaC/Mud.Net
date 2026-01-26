using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Damage | AbilityEffects.Debuff)]
[AbilityCharacterWearOffMessage("You feel less cold.")]
[AbilityDispellable("{0:N} looks warmer.")]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell inflicts damage on the victim and also reduces the victim's
strength by one.")]
[OneLineHelp("weakens your enemy with a frigid grasp")]
public class ChillTouch : DamageTableSpellBase
{
    private const string SpellName = "Chill Touch";

    private IAuraManager AuraManager { get; }

    public ChillTouch(ILogger<ChillTouch> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override SchoolTypes DamageType => SchoolTypes.Cold;
    protected override string DamageNoun => "chill touch";
    protected override int[] Table =>
    [
         0,
         0,  0,  6,  7,  8,  9, 12, 13, 13, 13,
        14, 14, 14, 15, 15, 15, 16, 16, 16, 17,
        17, 17, 18, 18, 18, 19, 19, 19, 20, 20,
        20, 21, 21, 21, 22, 22, 22, 23, 23, 23,
        24, 24, 24, 25, 25, 25, 26, 26, 26, 27
    ];

    protected override void Invoke()
    {
        base.Invoke();
        if (SavesSpellResult || DamageResult != DamageResults.Done)
            return;
        Victim.Act(ActOptions.ToRoom, "{0} turns blue and shivers.", Victim);
        var existingAura = Victim.GetAura(SpellName);
        if (existingAura != null)
        {
            existingAura.Update(Level, TimeSpan.FromMinutes(6));
            existingAura.AddOrUpdateAffect(
                x => x.Location == CharacterAttributeAffectLocations.Strength,
                () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                x => x.Modifier -= 1);
        }
        else
            AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(6), new AuraFlags(), true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add });
    }
}
