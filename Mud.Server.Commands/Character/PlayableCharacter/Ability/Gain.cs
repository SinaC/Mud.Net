using Mud.Common;
using Mud.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
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
        DisplaySpells,
        DisplaySkillsAndPassives,
        Convert,
        Revert,
        Gain
    }

    protected INonPlayableCharacter Trainer { get; set; } = default!;
    protected IAbilityLearned AbilityLearned { get; set; } = default!;
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

        // list
        if (StringCompareHelpers.StringStartsWith("list", actionInput.Parameters[0].Value))
        {
            Action = Actions.DisplayAll;
            return null;
        }
        // skills + passives
        if (StringCompareHelpers.StringStartsWith("skills", actionInput.Parameters[0].Value))
        {
            Action = Actions.DisplaySpells;
            return null;
        }
        // spells
        if (StringCompareHelpers.StringStartsWith("spells", actionInput.Parameters[0].Value))
        {
            Action = Actions.DisplaySkillsAndPassives;
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
        // Gain ability
        // TODO: search among all abilities even if can't be learned, not yet be learned, already learned ?
        var abilityInfo = AbilityManager.Search(actionInput.Parameters[0]);
        if (abilityInfo == null)
            return Actor.ActPhrase("{0:N} tells you 'I do not understand...'", Trainer);
        AbilityLearned = Actor.LearnedAbilities.FirstOrDefault(x => x.CanBeGained(Actor) && StringCompareHelpers.StringStartsWith(x.Name, actionInput.Parameters[0].Value))!;
        if (AbilityLearned == null)
            return Actor.ActPhrase("{0:N} tells you 'This is beyond your powers.'", Trainer);
        if (Actor.Trains < AbilityLearned.Rating)
            return Actor.ActPhrase("{0:N} tells you 'You are not yet ready for that ability.'", Trainer);
        Action = Actions.Gain;
        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        switch (Action)
        {
            case Actions.DisplayAll:
                {
                    StringBuilder sb = GainAbilityTableGenerator.Value.Generate("Abilities", 3, Actor.LearnedAbilities.Where(x => x.CanBeGained(Actor)).OrderBy(x => x.Level).ThenBy(x => x.Name));
                    Actor.Send(sb);
                    return;
                }
            case Actions.DisplaySpells:
                {
                    StringBuilder sb = GainAbilityTableGenerator.Value.Generate("Skills and Passives", 3, Actor.LearnedAbilities.Where(x => (x.AbilityInfo.Type == AbilityTypes.Skill || x.AbilityInfo.Type == AbilityTypes.Passive) && x.CanBeGained(Actor)).OrderBy(x => x.Level).ThenBy(x => x.Name));
                    Actor.Send(sb);
                    return;
                }
            case Actions.DisplaySkillsAndPassives:
                {
                    StringBuilder sb = GainAbilityTableGenerator.Value.Generate("Spells", 3, Actor.LearnedAbilities.Where(x => x.AbilityInfo.Type == AbilityTypes.Spell && x.CanBeGained(Actor)).OrderBy(x => x.Level).ThenBy(x => x.Name));
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
            case Actions.Gain:
                {
                    AbilityLearned.IncrementLearned(1);
                    Actor.UpdateTrainsAndPractices(-AbilityLearned.Rating, 0);
                    Actor.Act(ActOptions.ToCharacter, "{0:N} trains you in the art of {1}", Trainer, AbilityLearned);
                    return;
                }
        }
    }

    private static readonly Lazy<TableGenerator<IAbilityLearned>> GainAbilityTableGenerator = new(() =>
    {
        TableGenerator<IAbilityLearned> generator = new();
        generator.AddColumn("Name", 18, x => x.Name);
        generator.AddColumn("Lvl", 5, x => x.Level.ToString());
        generator.AddColumn("Cost", 5, x => x.Rating.ToString());
        return generator;
    });
}
