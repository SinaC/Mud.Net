using Mud.Common;
using Mud.Server.Ability;
using Mud.Server.Commands.Admin.Information;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Race;
using System.Diagnostics;
using System.Text;

namespace Mud.Server.Commands.Actor;

[Command("help", Priority = 0)]
[Syntax(
    "[help] <topic>")]
[Help(
@"[cmd] without any arguments shows a one-page command summary.

[cmd] <keyword> shows a page of help on that keyword.  The keywords include
all the commands, spells, skills, passives, races and classes listed in the game.")]
public class Help : ActorGameAction
{
    private const int ColumnCount = 4;
    private const string PassivesCategory = "Passives";
    private const string SkillsCategory = "Skills";
    private const string SpellsCategory = "Spells";
    private const string RacesCategory = "Races";
    private const string ClassesCategory = "Classes";

    private IAbilityManager AbilityManager { get; }
    private IRaceManager RaceManager { get; }
    private IClassManager ClassManager { get; }

    public Help(IAbilityManager abilityManager, IRaceManager raceManager, IClassManager classManager)
    {
        AbilityManager = abilityManager;
        RaceManager = raceManager;
        ClassManager = classManager;
    }

    private bool ShouldDisplayCategoriesAndCommands { get; set; }
    protected ICommandParameter Parameter { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length == 0)
            ShouldDisplayCategoriesAndCommands = true;
        else
        {
            ShouldDisplayCategoriesAndCommands = false;
            var parameter = actionInput.Parameters[0];
            if (parameter.Value.Length < 2)
                return "topic must contain at least 2 characters";
            Parameter = parameter;
        }
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        if (ShouldDisplayCategoriesAndCommands)
        {
            StringBuilder sb = new();
            sb.AppendLine("Available topics:");
            AppendCategoriesAndRelatedCommands(sb, x => true);
            Actor.Page(sb);
        }
        else
        {
            bool Filter(string x) => StringCompareHelpers.StringStartsWith(x, Parameter.Value);
            StringBuilder sb = new();
            // categories and command names
            AppendCategoriesAndRelatedCommands(sb, Filter);
            // commands/skills
            AppendCommandsExcludingSkills(sb, Parameter.Value);
            // skills
            AppendSkills(sb, Filter);
            // spells
            AppendAbilities<ISpell>(sb, Filter);
            // passives
            AppendAbilities<IPassive>(sb, Filter);
            // races
            AppendRaces(sb, Filter);
            // classes
            AppendClasses(sb, Filter);
            // TODO: help not related to commands nor abilities nor races/classes
            if (sb.Length > 0)
                Actor.Page(sb);
            else
                Actor.Send("No topic found.");
        }
    }

    private void AppendCategoriesAndRelatedCommands(StringBuilder sb, Func<string, bool> categoryFilterFunc)
    {
        var topics = GenerateTopics();
        foreach (var topicsByCategory in topics
            .Where(x => categoryFilterFunc(x.Category))
            .GroupBy(x => new { x.Category, x.CategoryPriority })
            .OrderBy(x => x.Key.CategoryPriority)
            .ThenBy(x => x.Key.Category))
        {
            if (!string.IsNullOrEmpty(topicsByCategory.Key.Category))
                sb.AppendLine("%W%" + topicsByCategory.Key.Category + ":%x%");
            int index = 0;
            foreach (var topic in topicsByCategory
                .OrderBy(x => x.NamePriority)
                .ThenBy(x => x.Name))
            {
                sb.AppendFormat("{0,-20}", topic.Name);
                if ((++index % ColumnCount) == 0)
                    sb.AppendLine();
            }
            if (index > 0 && index % ColumnCount != 0)
                sb.AppendLine();
        }
    }

    private void AppendCommandsExcludingSkills(StringBuilder sb, string prefix)
    {
        var iSkillType = typeof(ISkill);
        var filteredGameActionInfos = Actor.GameActions.GetByPrefix(prefix).Where(x => !x.Value.Hidden && !iSkillType.IsAssignableFrom(x.Value.CommandExecutionType)).Select(x => x.Value);
        foreach (var gameActionInfosByExecutionType in filteredGameActionInfos
            .GroupBy(x => x.CommandExecutionType) // avoid displaying multiple times the same commands
            .OrderBy(x => x.First().Name))
        {
            foreach (var gameActionInfo in gameActionInfosByExecutionType)
            {
                var names = gameActionInfo.Names.ToArray();
                var title = $"%C%{string.Join(", ", names)}%x%";
                sb.AppendLine(title);
                var commandNames = string.Join("|", names);
                var sbSyntax = BuildCommandSyntax(commandNames, gameActionInfo.Syntax, false);
                sb.Append(sbSyntax);
                sb.AppendLine();
                if (gameActionInfo.Help != null)
                {
                    var help = gameActionInfo.Help!.Replace("[cmd]", commandNames.ToUpperInvariant());
                    sb.AppendLine(help);
                    sb.AppendLine();
                }
            }
        }
    }

    private void AppendSkills(StringBuilder sb, Func<string, bool> nameFilterFunc)
    {
        // displayed as skill and replace [cmd] with command names
        var iSkillType = typeof(ISkill);
        foreach (var abilityInfo in AbilityManager.SearchAbilities<ISkill>().Where(x => nameFilterFunc(x.Name)).OrderBy(x => x.Name))
        {
            // search game action infos matching skill
            var gameActionInfos = Actor.GameActions.Where(x => x.Value.CommandExecutionType == abilityInfo.AbilityExecutionType);
            var commandNames = string.Join("|", gameActionInfos.Select(x => x.Key));
            sb.AppendLine($"%C%{abilityInfo.Name}%x% [Skill]");
            if (abilityInfo.Syntax != null)
            {
                foreach (string syntaxEntry in abilityInfo.Syntax)
                {
                    string enrichedSyntax = syntaxEntry.Replace("[cmd]", commandNames);
                    sb.AppendLine("Syntax: " + enrichedSyntax);
                }
                sb.AppendLine();
            }
            if (abilityInfo.Help != null)
            {
                var help = abilityInfo.Help!.Replace("[cmd]", commandNames);
                sb.AppendLine(help);
            }
            sb.AppendLine();
        }
    }

    private void AppendAbilities<TAbility>(StringBuilder sb, Func<string, bool> nameFilterFunc)
        where TAbility: class, IAbility
    {
        foreach (var abilityInfo in AbilityManager.SearchAbilities<TAbility>().Where(x => nameFilterFunc(x.Name)).OrderBy(x => x.Name))
        {
            sb.AppendLine($"%C%{abilityInfo.Name}%x% [{abilityInfo.Type}]");
            if (abilityInfo.Syntax != null)
            {
                foreach (string syntaxEntry in abilityInfo.Syntax)
                {
                    string enrichedSyntax = syntaxEntry.Replace("[spell]", $"'{abilityInfo.Name.ToLowerInvariant()}'");
                    sb.AppendLine("Syntax: " + enrichedSyntax);
                }
                sb.AppendLine();
            }
            if (abilityInfo.Help != null)
            {
                sb.AppendLine(abilityInfo.Help!);
            }
            if (abilityInfo.Syntax == null && abilityInfo.Help == null)
            {
                sb.AppendLine("no syntax nor help section for this");
            }
            sb.AppendLine();
        }
    }

    private void AppendRaces(StringBuilder sb, Func<string, bool> nameFilterFunc)
    {
        foreach (var raceInfo in RaceManager.PlayableRaces.Where(x => nameFilterFunc(x.Name)).OrderBy(x => x.Name))
        {
            sb.AppendLine($"%C%{raceInfo.Name.ToPascalCase()}%x% [Race]");
            if (raceInfo.Help != null)
            {
                sb.AppendLine(raceInfo.Help);
            }
            sb.AppendLine();
        }
    }

    private void AppendClasses(StringBuilder sb, Func<string, bool> nameFilterFunc)
    {
        foreach (var classInfo in ClassManager.Classes.Where(x => nameFilterFunc(x.Name)).OrderBy(x => x.Name))
        {
            sb.AppendLine($"%C%{classInfo.Name.ToPascalCase()}%x% [Class]");
            if (classInfo.Help != null)
            {
                sb.AppendLine(classInfo.Help);
            }
            sb.AppendLine();
        }
    }

    private IEnumerable<Topic> GenerateTopics()
    {
        // commands (excluding skills)
        var iSkillType = typeof(ISkill);
        var filteredGameActionInfos = Actor.GameActions.Where(x => !x.Value.Hidden && x.Value.Help != null && !iSkillType.IsAssignableFrom(x.Value.CommandExecutionType)).Select(x => x.Value);
        var gameActionTopics = filteredGameActionInfos
            .SelectMany(x => x.Names, (gai, name) => new { name, gai })
            .GroupBy(x => x.name, (name, group) => new { name, group.First().gai })
            .SelectMany(x => x.gai.Categories, (kv, category) => new { category, kv.name, priority = kv.gai.Priority })
            .Select(x => new Topic(x.category, 0, x.name, x.priority));
        // passives
        var iPassiveType = typeof(IPassive);
        var passiveTopics = AbilityManager.Abilities.Where(x => iPassiveType.IsAssignableFrom(x.AbilityExecutionType))
            .Select(x => new Topic(PassivesCategory, 1, x.Name, 0));
        // skills
        var skillTopics = AbilityManager.Abilities.Where(x => iSkillType.IsAssignableFrom(x.AbilityExecutionType))
            .Select(x => new Topic(SkillsCategory, 2, x.Name, 0));
        // spells
        var iSpellType = typeof(ISpell);
        var spellTopics = AbilityManager.Abilities.Where(x => iSpellType.IsAssignableFrom(x.AbilityExecutionType))
            .Select(x => new Topic(SpellsCategory, 3, x.Name, 0));
        // races
        var raceTopics = RaceManager.PlayableRaces
            .Select(x => new Topic(RacesCategory, 4, x.Name, 0));
        // classes
        var classesTopics = ClassManager.Classes
            .Select(x => new Topic(ClassesCategory, 5, x.Name, 0));
        // TODO: help not related to commands nor abilities nor races/classes
        return gameActionTopics.Concat(passiveTopics).Concat(skillTopics).Concat(spellTopics);
    }

    private record Topic(string Category, int CategoryPriority, string Name, int NamePriority);
}
