using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Flags;
using Mud.Server.GameAction;
using Mud.Server.Guards.Attributes;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff | AbilityEffects.Animation), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell, if successful, causes the victim to follow you and to take orders
from you.  Use ORDER to order your charmed followers.

You are responsible for the actions of your followers.  Conversely, other
people who attack your followers will be penalized as if they attacked you.")]
[OneLineHelp("turns an enemy into a trusted friend")]
public class CharmPerson : OffensiveSpellBase
{
    private const string SpellName = "Charm Person";

    private IAuraManager AuraManager { get; }

    public CharmPerson(ILogger<CharmPerson> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    public override string? Setup(ISpellActionInput spellActionInput)
    {
        var baseSetup = base.Setup(spellActionInput);
        if (baseSetup != null)
            return baseSetup;
        if (Caster is not IPlayableCharacter pcCaster)
            return "You can't charm!";
        return null;
    }

    protected override string? SetTargets(ISpellActionInput spellActionInput)
    {
        var baseSetTargets = base.SetTargets(spellActionInput);
        if (baseSetTargets != null)
            return baseSetTargets;

        var safeResult = Victim.IsSafe(Caster);
        if (safeResult != null)
            return safeResult;

        if (Caster == Victim)
            return "You like yourself even better!";

        if (Victim is not INonPlayableCharacter npcVictim)
            return "You can't charm players!";

        if (npcVictim.Room.RoomFlags.IsSet("Law"))
            return "The mayor does not allow charming in the city limits.";

        return null;
    }

    protected override void Invoke()
    {
        var npcVictim = (INonPlayableCharacter)Victim; // SetTargets ensure this will never failed
        if (npcVictim.CharacterFlags.IsSet("Charm")
            || Caster.CharacterFlags.IsSet("Charm")
            || Level < npcVictim.Level
            || npcVictim.Immunities.IsSet("Charm")
            || npcVictim.SavesSpell(Level, SchoolTypes.Charm))
            return;

        ((IPlayableCharacter)Caster).AddPet(npcVictim); // Guards ensure this will never failed

        int duration = RandomManager.Fuzzy(Level / 4);
        AuraManager.AddAura(npcVictim, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
            new CharacterFlagsAffect { Modifier = new CharacterFlags("Charm"), Operator = AffectOperators.Or });

        npcVictim.Act(ActOptions.ToCharacter, "Isn't {0} just so nice?", Caster);
        if (Caster != npcVictim)
            Caster.Act(ActOptions.ToCharacter, "{0:N} looks at you with adoring eyes.", npcVictim);
    }
}
