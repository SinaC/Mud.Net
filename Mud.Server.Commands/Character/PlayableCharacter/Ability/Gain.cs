using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability.AbilityGroup;
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
        "[cmd] skills|spells",
        "[cmd] convert|revert",
        "[cmd] <ability>")]
[Help(
@"The gain command is used to learn new skills, once the proper trainer has 
been found. (The sailor at Midgaard, From Recall : 2 South, 3 East, South) 
The following options can be used with gain:

gain list:    list all groups and skills/spells that can be learned
gain groups:  list all groups that can be learned
gain skills:  list all skills that can be learned
gain spells:  list all spells that can be learned
gain <name>:  add a skill or spell, at the listed cost
gain convert: turns 10 practices into one training session
gain revert:  turns 1 train into 10 practices

Gain uses training sessions, not practices. (see 'help train')
Gained skills and spells do NOT increase your experience per level or total
number of creation points.")]
public class Gain : PlayableCharacterGameAction
{
    private IAbilityManager AbilityManager { get; }
    private IAbilityGroupManager AbilityGroupManager { get; }

    public Gain(IAbilityManager abilityManager, IAbilityGroupManager abilityGroupManager)
    {
        AbilityManager = abilityManager;
        AbilityGroupManager = abilityGroupManager;
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
    protected IAbilityLearned AbilityLearned { get; set; } = default!;
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
        AbilityLearned = Actor.LearnedAbilities.FirstOrDefault(x => x.CanBeGained(Actor) && StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value))!;
        if (AbilityLearned == null)
            return Actor.ActPhrase("{0:N} tells you 'This is beyond your powers.'", Trainer);
        if (Actor.Trains < AbilityLearned.Rating)
            return Actor.ActPhrase("{0:N} tells you 'You are not yet ready for that ability.'", Trainer);
        Action = Actions.GainAbility;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        switch (Action)
        {
            case Actions.DisplayAll:
                {
                    var sbAbilities = GainAbilityTableGenerator.Value.Generate("Abilities", 3, Actor.LearnedAbilities.Where(x => x.CanBeGained(Actor)).OrderBy(x => x.Level).ThenBy(x => x.Name));
                    var availableAbilityGroupsNotYetLearned = GetAvailableAbilityGroupsNotYetLearned();
                    var sbGroups = GainAbilityGroupTableGenerator.Value.Generate("Groups", 3, availableAbilityGroupsNotYetLearned.OrderBy(x => x.Name));
                    var sb = new StringBuilder();
                    sb.Append(sbAbilities);
                    sb.Append(sbGroups);
                    Actor.Send(sb);
                    return;
                }
            case Actions.DisplayGroups:
                {
                    var availableAbilityGroupsNotYetLearned = GetAvailableAbilityGroupsNotYetLearned();
                    StringBuilder sb = GainAbilityGroupTableGenerator.Value.Generate("Groups", 3, availableAbilityGroupsNotYetLearned.OrderBy(x => x.Name));
                    Actor.Send(sb);
                    return;
                }
            case Actions.DisplaySpells:
                {
                    StringBuilder sb = GainAbilityTableGenerator.Value.Generate("Spells", 3, Actor.LearnedAbilities.Where(x => x.AbilityDefinition.Type == AbilityTypes.Spell && x.CanBeGained(Actor)).OrderBy(x => x.Level).ThenBy(x => x.Name));
                    Actor.Send(sb);
                    return;
                }
            case Actions.DisplaySkillsAndPassivesAndWeapons:
                {
                    StringBuilder sb = GainAbilityTableGenerator.Value.Generate("Skills/Passives/Weapons", 3, Actor.LearnedAbilities.Where(x => (x.AbilityDefinition.Type == AbilityTypes.Skill || x.AbilityDefinition.Type == AbilityTypes.Passive || x.AbilityDefinition.Type == AbilityTypes.Weapon) && x.CanBeGained(Actor)).OrderBy(x => x.Level).ThenBy(x => x.Name));
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
                    AbilityLearned.IncrementLearned(1);
                    Actor.UpdateTrainsAndPractices(-AbilityLearned.Rating, 0);
                    Actor.Act(ActOptions.ToCharacter, "{0:N} trains you in the art of {1}", Trainer, AbilityLearned.Name);
                    return;
                }
        }
    }

    private IEnumerable<IAbilityGroupUsage> GetAvailableAbilityGroupsNotYetLearned()
        => Actor.Class.AvailableAbilityGroups.Where(x => Actor.LearnedAbilityGroups.All(y => !StringCompareHelpers.StringEquals(y.Name, x.Name)));

    private static readonly Lazy<TableGenerator<IAbilityLearned>> GainAbilityTableGenerator = new(() =>
    {
        TableGenerator<IAbilityLearned> generator = new();
        generator.AddColumn("Name", 18, x => x.Name);
        generator.AddColumn("Lvl", 5, x => x.Level.ToString());
        generator.AddColumn("Cost", 5, x => x.Rating.ToString());
        return generator;
    });

    private static readonly Lazy<TableGenerator<IAbilityGroupUsage>> GainAbilityGroupTableGenerator = new(() =>
    {
        TableGenerator<IAbilityGroupUsage> generator = new();
        generator.AddColumn("Name", 18, x => x.Name);
        generator.AddColumn("Cost", 5, x => x.Cost.ToString());
        return generator;
    });
}
