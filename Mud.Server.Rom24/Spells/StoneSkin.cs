using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 18)]
[AbilityCharacterWearOffMessage("Your skin feels soft again.")]
[AbilityDispellable("{0:N}'s skin regains its normal texture.")]
[Syntax("cast [spell]")]
[Help(
@"These spells protect the caster by decreasing (improving) the caster's armor
class. It provides 40 points of armor.")]
[OneLineHelp("turns skin hard as stone, providing a huge armor boost")]
public class StoneSkin : DefensiveSpellBase
{
    private const string SpellName = "Stone Skin";

    private IAuraManager AuraManager { get; }

    public StoneSkin(ILogger<StoneSkin> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Victim.GetAura(SpellName) != null)
        {
            if (Victim == Caster)
                Caster.Send("Your skin is already as hard as a rock.");
            else
                Caster.Act(ActOptions.ToCharacter, "{0:N} is already as hard as can be.", Victim);
            return;
        }

        AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(Level), AuraFlags.None, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -40, Operator = AffectOperators.Add });
        Caster.Act(ActOptions.ToAll, "{0:P} skin turns to stone.", Victim);
    }
}
