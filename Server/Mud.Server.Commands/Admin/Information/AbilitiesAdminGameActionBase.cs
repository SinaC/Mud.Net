using Mud.Common;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Guards;
using Mud.Server.Interfaces.Race;
using Mud.Server.TableGenerator;

namespace Mud.Server.Commands.Admin.Information;

public abstract class AbilitiesAdminGameActionBase : AdminGameAction
{
    protected override IGuard<IAdmin>[] Guards => [];

    private IAbilityManager AbilityManager { get; }
    protected IClassManager ClassManager { get; }
    protected IRaceManager RaceManager { get; }

    protected AbilitiesAdminGameActionBase(IAbilityManager abilityManager, IClassManager classManager, IRaceManager raceManager)
    {
        AbilityManager = abilityManager;
        ClassManager = classManager;
        RaceManager = raceManager;
    }

    protected IClass Class { get; set; } = default!;
    protected IPlayableRace Race { get; set; } = default!;

    protected abstract AbilityTypes? AbilityTypesFilter { get; }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (actionInput.Parameters.Length > 0)
        {
            // filter on class?
            Class = ClassManager.Classes.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value))!;
            if (Class != null)
                return null;
            // filter on race?
            Race = RaceManager.PlayableRaces.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value))!;
            if (Race == null)
                return BuildCommandSyntax();
        }
        // no filter
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        string title;
        if (AbilityTypesFilter.HasValue)
        {
            title = AbilityTypesFilter.Value switch
            {
                AbilityTypes.Weapon => "Weapons",
                AbilityTypes.Passive => "Passives",
                AbilityTypes.Spell => "Spells",
                AbilityTypes.Skill => "Skills",
                _ => "???",
            };
        }
        else
            title = "Abilities";
        if (Class == null && Race == null)
        {
            // no filter
            var sb = TableGenerators.FullInfoAbilityTableGenerator.Value.Generate(title, AbilityManager.Abilities
                .Where(x => !AbilityTypesFilter.HasValue || x.Type == AbilityTypesFilter.Value)
                .OrderBy(x => x.Name));
            Actor.Page(sb);
            return;
        }

        // filter on class?
        if (Class != null)
        {
            var sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate($"{title} for {Class.DisplayName}", Class.AvailableAbilities
                .Where(x => !AbilityTypesFilter.HasValue || x.AbilityDefinition.Type == AbilityTypesFilter.Value)
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Name));
            Actor.Page(sb);
            return;
        }

        // filter on race?
        if (Race != null)
        {
            var sb = TableGenerators.FullInfoAbilityUsageTableGenerator.Value.Generate($"{title} for {Race.DisplayName}", Race.Abilities
                .Where(x => !AbilityTypesFilter.HasValue || x.AbilityDefinition.Type == AbilityTypesFilter.Value)
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Name));
            Actor.Page(sb);
            return;
        }
    }
}
