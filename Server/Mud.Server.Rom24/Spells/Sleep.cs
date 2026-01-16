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
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff), NotInCombat(Message = StringHelpers.YouLostYourConcentration)]
[AbilityCharacterWearOffMessage("You feel less tired.")]
[AbilityDispellable]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell puts its victim to sleep.")]
[OneLineHelp("puts a foe into enchanted slumber")]
public class Sleep : OffensiveSpellBase
{
    private const string SpellName = "Sleep";

    private IAuraManager AuraManager { get; }

    public Sleep(ILogger<Sleep> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Victim.CharacterFlags.IsSet("Sleep")
            || (Victim is INonPlayableCharacter npcVictim && npcVictim.ActFlags.IsSet("Undead"))
            || Level + 2 < Victim.Level
            || Victim.SavesSpell(Level - 4, SchoolTypes.Charm))
            return;

        AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(4 + Level), AuraFlags.None, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -10, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags("Sleep"), Operator = AffectOperators.Or });

        if (Victim.Position > Positions.Sleeping)
        {
            Victim.Send("You feel very sleepy ..... zzzzzz.");
            Victim.Act(ActOptions.ToRoom, "{0:N} goes to sleep.", Victim);
            Victim.ChangePosition(Positions.Sleeping);
        }
    }
}
