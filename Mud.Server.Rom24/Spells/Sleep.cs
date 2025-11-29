using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff)]
[AbilityCharacterWearOffMessage("You feel less tired.")]
[AbilityDispellable]
[Syntax("cast [spell] <victim>")]
[Help(
@"This spell puts its victim to sleep.")]
public class Sleep : OffensiveSpellBase
{
    private const string SpellName = "Sleep";

    private IFlagFactory<ICharacterFlags, ICharacterFlagValues> CharacterFlagFactory { get; }
    private IAuraManager AuraManager { get; }

    public Sleep(ILogger<Sleep> logger, IFlagFactory<ICharacterFlags, ICharacterFlagValues> characterFlagFactory, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        CharacterFlagFactory = characterFlagFactory;
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
            new CharacterFlagsAffect { Modifier = CharacterFlagFactory.CreateInstance("Sleep"), Operator = AffectOperators.Or });

        if (Victim.Position > Positions.Sleeping)
        {
            Victim.Send("You feel very sleepy ..... zzzzzz.");
            Victim.Act(ActOptions.ToRoom, "{0:N} goes to sleep.", Victim);
            Victim.ChangePosition(Positions.Sleeping);
        }
    }
}
