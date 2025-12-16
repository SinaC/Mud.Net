using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.AbilityGroup;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.TableGenerator;
using System.Text;

namespace Mud.Server.Commands.Character.PlayableCharacter.Ability;

[PlayableCharacterCommand("gain", "Ability", MinPosition = Positions.Standing, NotInCombat = true)]
[Syntax(
        "[cmd] list",
        "[cmd] skills|spells|groups",
        "[cmd] convert|revert",
        "[cmd] <ability|group>")]
[Help(
@"The gain command is used to learn new skills, once the proper trainer has 
been found. (The sailor at Midgaard, From Recall : 2 South, 3 East, South) 
The following options can be used with gain:

gain list:    list all groups and skills/spells that can be learned
gain skills:  list all skills that can be learned
gain spells:  list all spells that can be learned
gain groups:  list all groups that can be learned
gain <name>:  add a skill or spell or a group, at the listed cost
gain convert: turns 10 practices into one training session
gain revert:  turns 1 train into 10 practices

Gain uses training sessions, not practices. (see 'help train')
Gained skills and spells do NOT increase your experience per level or total
number of creation points.")]
public class Gain : PlayableCharacterGameAction
{
    private IAbilityManager AbilityManager { get; }

    public Gain(IAbilityManager abilityManager)
    {
        AbilityManager = abilityManager;
    }

    protected enum Actions
    {
        DisplayAll,
        DisplayGroups,
        DisplaySpells,
        DisplaySkillsAndPassivesAndWeapons,
        Convert,
        Revert,
        GainAbilityGroup,
        GainAbility
    }

    protected INonPlayableCharacter Trainer { get; set; } = default!;
    protected IAbilityUsage AbilityUsage { get; set; } = default!;
    protected IAbilityGroupUsage AbilityGroupUsage { get; set; } = default!;
    protected Actions Action { get; set; }
    
    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        Trainer = Actor.Room?.NonPlayableCharacters.FirstOrDefault(x => Actor.CanSee(x) && x.ActFlags.IsSet("Gain"))!;
        if (Trainer == null)
            return "You can't do that here.";

        if (actionInput.Parameters.Length == 0)
            return Actor.ActPhrase("{0:N} tells you 'Pardon me?'", Trainer);

        if (actionInput.Parameters[0].IsAll)
            return "You can't gain that.";

        // list
        if (StringCompareHelpers.StringStartsWith("list", actionInput.Parameters[0].Value))
        {
            Action = Actions.DisplayAll;
            return null;
        }
        // groups
        if (StringCompareHelpers.StringStartsWith("groups", actionInput.Parameters[0].Value))
        {
            Action = Actions.DisplayGroups;
            return null;
        }
        // skills + passives + weapons
        if (StringCompareHelpers.StringStartsWith("skills", actionInput.Parameters[0].Value))
        {
            Action = Actions.DisplaySkillsAndPassivesAndWeapons;
            return null;
        }
        // spells
        if (StringCompareHelpers.StringStartsWith("spells", actionInput.Parameters[0].Value))
        {
            Action = Actions.DisplaySpells;
            return null;
        }
        // convert
        if (StringCompareHelpers.StringStartsWith("convert", actionInput.Parameters[0].Value))
        {
            if (Actor.Practices < 10)
                return Actor.ActPhrase("{0:N} tells you 'You are not yet ready.'", Trainer);
            Action = Actions.Convert;
            return null;
        }
        // revert
        if (StringCompareHelpers.StringStartsWith("revert", actionInput.Parameters[0].Value))
        {
            if (Actor.Trains < 1)
                return Actor.ActPhrase("{0:N} tells you 'You are not yet ready.'", Trainer);
            Action = Actions.Revert;
            return null;
        }
        // gain ability group
        var abilityGroupUsage = GetAvailableAbilityGroupsNotYetLearned().FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, actionInput.Parameters[0].Value));
        if (abilityGroupUsage != null)
        {
            if (Actor.Trains < abilityGroupUsage.Cost)
                return Actor.ActPhrase("{0:N} tells you 'You are not yet ready for that group.'", Trainer);
            AbilityGroupUsage = abilityGroupUsage;
            Action = Actions.GainAbilityGroup;
            return null;
        }
        // gain ability
        // TODO: search among all abilities even if can't be learned, not yet be learned, already learned ?
        var abilityDefinition = AbilityManager.Search(actionInput.Parameters[0]);
        if (abilityDefinition == null)
            return Actor.ActPhrase("{0:N} tells you 'I do not understand...'", Trainer);
        var availableAbilitiesNotYetLearned = GetAvailableAbilitiesNotYetLearned(null);
        var abilityUsage = availableAbilitiesNotYetLearned.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value))!;
        if (abilityUsage == null)
            return Actor.ActPhrase("{0:N} tells you 'This is beyond your powers.'", Trainer);
        if (Actor.Trains < abilityUsage.Rating)
            return Actor.ActPhrase("{0:N} tells you 'You are not yet ready for that ability.'", Trainer);
        AbilityUsage = abilityUsage;
        Action = Actions.GainAbility;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        switch (Action)
        {
            case Actions.DisplayAll:
                {
                    var availableAbilitiesNotYetLearned = GetAvailableAbilitiesNotYetLearned(null);
                    var sbAbilities = TableGenerators.AbilityUsageTableGenerator.Value.Generate("Abilities", new TableGeneratorOptions { ColumnRepetionCount = 3 }, availableAbilitiesNotYetLearned.OrderBy(x => x.Level).ThenBy(x => x.Name));
                    var availableAbilityGroupsNotYetLearned = GetAvailableAbilityGroupsNotYetLearned();
                    var sbGroups = TableGenerators.AbilityGroupUsageTableGenerator.Value.Generate("Groups", new TableGeneratorOptions { ColumnRepetionCount = 3 }, availableAbilityGroupsNotYetLearned.OrderBy(x => x.Name));
                    var sb = new StringBuilder();
                    sb.Append(sbAbilities);
                    sb.Append(sbGroups);
                    Actor.Send(sb);
                    return;
                }
            case Actions.DisplayGroups:
                {
                    var availableAbilityGroupsNotYetLearned = GetAvailableAbilityGroupsNotYetLearned();
                    StringBuilder sb = TableGenerators.AbilityGroupUsageTableGenerator.Value.Generate("Groups", new TableGeneratorOptions { ColumnRepetionCount = 3 }, availableAbilityGroupsNotYetLearned.OrderBy(x => x.Name));
                    Actor.Send(sb);
                    return;
                }
            case Actions.DisplaySpells:
                {
                    var availableAbilitiesNotYetLearned = GetAvailableAbilitiesNotYetLearned(x => x.AbilityDefinition.Type == AbilityTypes.Spell);
                    var sb = TableGenerators.AbilityUsageTableGenerator.Value.Generate("Spells", new TableGeneratorOptions { ColumnRepetionCount = 3 }, availableAbilitiesNotYetLearned.OrderBy(x => x.Level).ThenBy(x => x.Name));
                    Actor.Send(sb);
                    return;
                }
            case Actions.DisplaySkillsAndPassivesAndWeapons:
                {
                    var availableAbilitiesNotYetLearned = GetAvailableAbilitiesNotYetLearned(x => x.AbilityDefinition.Type == AbilityTypes.Skill || x.AbilityDefinition.Type == AbilityTypes.Passive || x.AbilityDefinition.Type == AbilityTypes.Weapon);
                    var sb = TableGenerators.AbilityUsageTableGenerator.Value.Generate("Skills/Passives/Weapons", new TableGeneratorOptions { ColumnRepetionCount = 3 }, availableAbilitiesNotYetLearned.OrderBy(x => x.Level).ThenBy(x => x.Name));
                    Actor.Send(sb);
                    return;
                }
            case Actions.Convert:
                {
                    Actor.UpdateTrainsAndPractices(1, -10);
                    Actor.Act(ActOptions.ToCharacter, "{0:N} helps you apply your practice to training", Trainer);
                    return;
                }
            case Actions.Revert:
                {
                    Actor.UpdateTrainsAndPractices(-1, 10);
                    Actor.Act(ActOptions.ToCharacter, "{0:N} helps you apply your train to practicing", Trainer);
                    return;
                }
            case Actions.GainAbilityGroup:
                {
                    Actor.AddLearnedAbilityGroup(AbilityGroupUsage);
                    Actor.UpdateTrainsAndPractices(-AbilityGroupUsage.Cost, 0);
                    Actor.Act(ActOptions.ToCharacter, "{0:N} trains you in the art of {1}", Trainer, AbilityGroupUsage.Name);
                    return;
                }
            case Actions.GainAbility:
                {
                    Actor.AddLearnedAbility(AbilityUsage);
                    Actor.UpdateTrainsAndPractices(-AbilityUsage.Rating, 0);
                    Actor.Act(ActOptions.ToCharacter, "{0:N} trains you in the art of {1}", Trainer, AbilityUsage.Name);
                    return;
                }
        }
    }

    private IEnumerable<IAbilityGroupUsage> GetAvailableAbilityGroupsNotYetLearned()
        => Actor.Class.AvailableAbilityGroups.Where(x => Actor.LearnedAbilityGroups.All(y => !StringCompareHelpers.StringEquals(y.Name, x.Name)));

    private IEnumerable<IAbilityUsage> GetAvailableAbilitiesNotYetLearned(Func<IAbilityUsage, bool>? filter)
        => Actor.Class.AvailableAbilities.Where(x => (filter == null || filter(x)) && Actor.LearnedAbilities.All(y => !StringCompareHelpers.StringEquals(y.Name, x.Name)));
}
