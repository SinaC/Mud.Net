using Mud.Common;
using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Race;
using System.Linq;
using System.Text;

namespace Mud.Server.Admin.Information
{
    [AdminCommand("info", "Information")]
    [Syntax(
        "[cmd] race",
        "[cmd] class",
        "[cmd] <race>",
        "[cmd] <class>")]
    public class Info : AdminGameAction
    {
        private IClassManager ClassManager { get; }
        private IRaceManager RaceManager { get; }

        public bool DisplayClasses { get; protected set; }
        public bool DisplayRaces { get; protected set; }
        public IClass Class { get; protected set; }
        public IPlayableRace Race { get; protected set; }

        public Info(IClassManager classManager, IRaceManager raceManager)
        {
            ClassManager = classManager;
            RaceManager = raceManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return BuildCommandSyntax();

            if (actionInput.Parameters[0].Value == "class" || actionInput.Parameters[0].Value == "classes")
            {
                DisplayClasses = true;
                return null;
            }

            if (actionInput.Parameters[0].Value == "race" || actionInput.Parameters[0].Value == "races")
            {
                DisplayRaces = true;
                return null;
            }

            Class = ClassManager.Classes.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value));
            if (Class != null)
                return null;

            Race = RaceManager.PlayableRaces.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value));
            if (Race != null)
                return null;

            return BuildCommandSyntax();
        }

        public override void Execute(IActionInput actionInput)
        {
            if (DisplayClasses)
            {
                StringBuilder sb = TableGenerators.ClassTableGenerator.Value.Generate("Classes", ClassManager.Classes);
                Actor.Page(sb);
                return;
            }

            if (DisplayRaces)
            {
                StringBuilder sb = TableGenerators.PlayableRaceTableGenerator.Value.Generate("Races", RaceManager.PlayableRaces);
                Actor.Page(sb);
                return;
            }

            if (Class != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate(
                    new[]
                    {
                        Class.DisplayName,
                        $"ShortName: {Class.ShortName}",
                        $"Resource(s): {string.Join(",", Class.ResourceKinds?.Select(x => $"{x.ResourceColor()}") ?? Enumerable.Empty<string>())}",
                        $"Prime attribute: %W%{Class.PrimeAttribute}%x%",
                        $"Max practice percentage: %W%{Class.MaxPracticePercentage}%x%",
                        $"Hp/level: min: %W%{Class.MinHitPointGainPerLevel}%x% max: %W%{Class.MaxHitPointGainPerLevel}%x%"
                    },
                    Class.Abilities.OrderBy(x => x.Level).ThenBy(x => x.Name));
                Actor.Page(sb);
                return;
            }

            if (Race != null)
            {
                StringBuilder sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate(
                    new[]
                    {
                        Race.DisplayName,
                        $"ShortName: {Race.ShortName}",
                        $"Immunities: %y%{Race.Immunities}%x%",
                        $"Resistances: %b%{Race.Resistances}%x%",
                        $"Vulnerabilities: %r%{Race.Vulnerabilities}%x%",
                        $"Size: %M%{Race.Size}%x%",
                        $"Exp/Level:     %W%{string.Join(" ", ClassManager.Classes.Select(x => $"{x.ShortName,5}"))}%x%",
                        $"               %r%{string.Join(" ", ClassManager.Classes.Select(x => $"{Race.ClassExperiencePercentageMultiplier(x)*10,5}"))}%x%", // *10 because base experience is 1000
                        $"Attributes:       %Y%{string.Join(" ", EnumHelpers.GetValues<BasicAttributes>().Select(x => $"{x.ShortName(),3}"))}%x%",
                        $"Attributes start: %c%{string.Join(" ", EnumHelpers.GetValues<BasicAttributes>().Select(x => $"{Race.GetStartAttribute((CharacterAttributes)x),3}"))}%x%",
                        $"Attributes max:   %B%{string.Join(" ", EnumHelpers.GetValues<BasicAttributes>().Select(x => $"{Race.GetMaxAttribute((CharacterAttributes)x),3}"))}%x%",
                    },
                    Race.Abilities.OrderBy(x => x.Level).ThenBy(x => x.Name));
                Actor.Page(sb);
            }
        }
    }
}
