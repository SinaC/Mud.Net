using Mud.Common;
using Mud.Server.Common.Extensions;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.AdminGuards;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Race;
using Mud.Server.TableGenerator;
using System.Text;

namespace Mud.Server.Commands.Admin.Information;

[AdminCommand("info", "Information")]
[Syntax(
    "[cmd] race",
    "[cmd] npcrace",
    "[cmd] class",
    "[cmd] <race>",
    "[cmd] <npcrace>",
    "[cmd] <class>")]
public class Info : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [new RequiresAtLeastOneArgument()];

    private IClassManager ClassManager { get; }
    private IRaceManager RaceManager { get; }

    public Info(IClassManager classManager, IRaceManager raceManager)
    {
        ClassManager = classManager;
        RaceManager = raceManager;
    }

    private bool DisplayClasses { get; set; }
    private bool DisplayRaces { get; set; }
    private bool DisplayNpcRaces { get; set; }
    private IClass Class { get; set; } = default!;
    private IPlayableRace Race { get; set; } = default!;
    private IRace NpcRace { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

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

        if (actionInput.Parameters[0].Value == "npcrace" || actionInput.Parameters[0].Value == "npcraces")
        {
            DisplayNpcRaces = true;
            return null;
        }

        Class = ClassManager.Classes.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value))!;
        if (Class != null)
            return null;

        Race = RaceManager.PlayableRaces.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value))!;
        if (Race != null)
            return null;

        NpcRace = RaceManager.Races.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value))!;
        if (NpcRace != null)
            return null;

        return BuildCommandSyntax();
    }

    public override void Execute(IActionInput actionInput)
    {
        if (DisplayClasses)
        {
            var sb = TableGenerators.ClassTableGenerator.Value.Generate("Classes", ClassManager.Classes);
            Actor.Page(sb);
            return;
        }

        if (DisplayRaces)
        {
            var sb = TableGenerators.PlayableRaceTableGenerator.Value.Generate("Races", RaceManager.PlayableRaces);
            Actor.Page(sb);
            return;
        }

        if (DisplayNpcRaces)
        {
            var sb = TableGenerators.RaceTableGenerator.Value.Generate("NPC Races", new TableGeneratorOptions { ColumnRepetionCount = 2 }, RaceManager.Races);
            Actor.Page(sb);
            return;
        }

        if (Class != null)
        {
            var sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate(
                [
                    Class.DisplayName,
                    $"ShortName: {Class.ShortName}",
                    $"Resource(s): {string.Join(",", Class.ResourceKinds?.Select(x => $"{x.ResourceColor(false)}") ?? [])}",
                    $"Prime attribute: %W%{Class.PrimeAttribute}%x%",
                    $"Max practice percentage: %W%{Class.MaxPracticePercentage}%x%",
                    $"Hp/level: min: %W%{Class.MinHitPointGainPerLevel}%x% max: %W%{Class.MaxHitPointGainPerLevel}%x%"
                ],
                Class.AvailableAbilities.OrderBy(x => x.Level).ThenBy(x => x.Name));
            Actor.Page(sb);
            return;
        }

        if (Race != null)
        {
            var sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate(
                [
                    Race.DisplayName,
                    $"ShortName: {Race.ShortName}",
                    $"Size: %M%{Race.Size}%x%",
                    $"Affects: %C%{Race.CharacterFlags}%x%",
                    $"Immunities: %y%{Race.Immunities}%x%",
                    $"Resistances: %b%{Race.Resistances}%x%",
                    $"Vulnerabilities: %r%{Race.Vulnerabilities}%x%",
                    $"Exp/Level:     %W%{string.Join(" ", ClassManager.Classes.Select(x => $"{x.ShortName,5}"))}%x%",
                    $"               %r%{string.Join(" ", ClassManager.Classes.Select(x => $"{Race.ClassExperiencePercentageMultiplier(x)*10,5}"))}%x%", // *10 because base experience is 1000
                    $"Attributes:       %Y%{string.Join(" ", Enum.GetValues<BasicAttributes>().Select(x => $"{x.ShortName(),3}"))}%x%",
                    $"Attributes start: %c%{string.Join(" ", Enum.GetValues<BasicAttributes>().Select(x => $"{Race.GetStartAttribute(x),3}"))}%x%",
                    $"Attributes max:   %B%{string.Join(" ", Enum.GetValues<BasicAttributes>().Select(x => $"{Race.GetMaxAttribute(x),3}"))}%x%",
                    $"Forms: {Race.BodyForms}",
                    $"Parts: {Race.BodyParts}",
                ],
                Race.Abilities.OrderBy(x => x.Level).ThenBy(x => x.Name));
            Actor.Page(sb);
            return;
        }

        if (NpcRace != null)
        {
            // TODO: better layout
            var sb = new StringBuilder();
            sb.AppendLine(NpcRace.DisplayName);
            sb.AppendLine($"Size: %M%{NpcRace.Size}%x%");
            sb.AppendLine($"Affects: %C%{NpcRace.CharacterFlags}%x%");
            sb.AppendLine($"Immunities: %y%{NpcRace.Immunities}%x%");
            sb.AppendLine($"Resistances: %b%{NpcRace.Resistances}%x%");
            sb.AppendLine($"Vulnerabilities: %r%{NpcRace.Vulnerabilities}%x%");
            sb.AppendLine($"Act: %W%{NpcRace.ActFlags}%x%");
            sb.AppendLine($"Offensive: %W%{NpcRace.OffensiveFlags}%x%");
            sb.AppendLine($"Assist: %W%{NpcRace.AssistFlags}%x%");
            sb.AppendLine($"Forms: {NpcRace.BodyForms}");
            sb.AppendLine($"Parts: {NpcRace.BodyParts}");
            Actor.Page(sb);
            return;
        }
    }
}
