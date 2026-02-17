using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Ability.Skill;
using Mud.Server.Ability.Skill.Interfaces;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.POC.Skills;

[CharacterCommand("morph", "Ability", "Buff", "Shapeshift", "POC")]
[Skill(SkillName, AbilityEffects.Buff, PulseWaitTime = 20)]
public class Morph : SkillBase
{
    private const string SkillName = "Morph";

    private static readonly string[] Vowels = ["a", "e", "i", "o", "u", "y"];

    private IAuraManager AuraManager { get; }
    private IRaceManager RaceManager { get; }

    public Morph(ILogger<BearForm> logger, IRandomManager randomManager, IAuraManager auraManager, IRaceManager raceManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
        RaceManager = raceManager;
    }

    protected override IGuard<ICharacter>[] Guards => [new CannotBeInCombat()];

    protected override bool MustBeLearned => true;

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        if (skillActionInput.Parameters.Length == 0)
            Race = null;
        else
        {
            var race = RaceManager.PlayableRaces.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, skillActionInput.Parameters[0].Value));
            if (race == null)
                return "This is not a valid race";
            Race = race;
        }
        return null;
    }

    private IPlayableRace? Race { get; set; }

    protected override bool Invoke()
    {
        if (Race == null)
        {
            User.Send("%W%You regain your original form%x%");
            User.Act(ActOptions.ToRoom, "%W%{0:N} regains {0:s} original form.%x%", this);
            User.RemoveAuras(x => StringCompareHelpers.StringEquals(x.AbilityName, SkillName), true, false);
            return true;
        }

        // TODO: better wording
        var article = Vowels.Any(Race.DisplayName.StartsWith)
            ? "an"
            : "a";
        User.Send($"%W%You morph into {article} {Race.DisplayName}.%x%");
        User.Act(ActOptions.ToRoom, "%W%{0:N} morphs into {1} {2}.%x%", this, article, Race.DisplayName);
        // remove previous morph without recompute if any
        User.RemoveAuras(x => StringCompareHelpers.StringEquals(x.AbilityName, SkillName), false, false);
        // apply new morph
        AuraManager.AddAura(User, SkillName, User, User.Level, new AuraFlags("NoDispel", "Permanent", "Shapeshift"), true,
            new CharacterRaceAffect(RaceManager) { Race = Race },
            new CharacterSizeAffect { Value = Race.Size },
            new CharacterFlagsAffect { Modifier = Race.CharacterFlags, Operator = AffectOperators.Assign },
            new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Modifier = Race.Immunities, Operator = AffectOperators.Assign },
            new CharacterIRVAffect { Location = IRVAffectLocations.Resistances, Modifier = Race.Resistances, Operator = AffectOperators.Assign },
            new CharacterIRVAffect { Location = IRVAffectLocations.Vulnerabilities, Modifier = Race.Vulnerabilities, Operator = AffectOperators.Assign },
            new CharacterBodyFormsAffect { Modifier = Race.BodyForms, Operator = AffectOperators.Assign },
            new CharacterBodyPartsAffect { Modifier = Race.BodyParts, Operator = AffectOperators.Assign });

        return true;
    }
}
