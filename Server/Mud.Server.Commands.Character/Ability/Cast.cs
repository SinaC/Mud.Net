using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Ability.Spell;
using Mud.Server.Ability.Spell.Interfaces;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Guards.CharacterGuards;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Commands.Character.Ability;

[CharacterCommand("cast", "Ability", Priority = 2)]
[Syntax("[cmd] <ability> <target>")]
[Help(
@"Before you can cast a spell, you have to practice it.  The more you practice,
the higher chance you have of success when casting.  Casting spells costs mana.
The mana cost decreases as your level increases.

The <target> is optional.  Many spells which need targets will use an
appropriate default target, especially during combat.

The <casting level> is optional. Many spells have casting level higher than 1,
and you don't have to cast spells at maximum casting level.

If the spell name is more than one word, then you must quote the spell name.
Example: cast 'cure critic' frag.  Quoting is optional for single-word spells.
You can abbreviate the spell name.

When you cast an offensive spell, the victim usually gets a saving throw.
The effect of the spell is reduced or eliminated if the victim makes the
saving throw successfully.

Some examples:
  cast 'chill touch' victim   will cast 'chill touch' on victim
  cast armor newbie           will cast armor on newbie
  cast fireball               will cast fireball on current target if already 
    in combat
  cast 'floating disc'        will cast 'floating disc' on caster

See also the help sections for individual spells.
Use the 'spells' command to see the spells you already have (help spells).")]
public class Cast : CharacterGameAction
{
    protected override IGuard<ICharacter>[] Guards => [new RequiresMinPosition(Positions.Sitting), new RequiresAtLeastOneArgument { Message = "Cast what ?" }];

    private ILogger<Cast> Logger { get; }
    private IAbilityManager AbilityManager { get; }

    public Cast(ILogger<Cast> logger, IAbilityManager abilityManager)
    {
        Logger = logger;
        AbilityManager = abilityManager;
    }

    private ISpell SpellInstance { get; set; } = default!;

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var (learned, abilityDefinition) = SearchLearnedSpell(actionInput.Parameters[0]);
        if (abilityDefinition == null || learned <= 0)
            return "You don't know any spells of that name.";

        SpellInstance = AbilityManager.CreateInstance<ISpell>(abilityDefinition.Name)!;
        if (SpellInstance == null)
        {
            Logger.LogError("Spell {abilityName} cannot be created.", abilityDefinition.Name);
            return StringHelpers.SomethingGoesWrong;
        }

        var newParameters = actionInput.Parameters.Skip(1).ToArray();
        var spellActionInput = new SpellActionInput(abilityDefinition, Actor, Actor.Level, newParameters);
        var spellInstanceGuards = SpellInstance.Setup(spellActionInput);
        return spellInstanceGuards;
    }

    public override void Execute(IActionInput actionInput)
    {
        SpellInstance.Execute();
    }

    private (int learned, IAbilityDefinition?) SearchLearnedSpell(ICommandParameter commandParameter)
    {
        var abilityDefinitions = AbilityManager.Search(commandParameter, AbilityTypes.Spell);
        foreach (var abilityDefinition in abilityDefinitions)
        {
            var (learned, abilityLearned) = Actor.GetAbilityLearnedAndPercentage(abilityDefinition.Name);
            if (abilityLearned != null)
                return (learned, abilityLearned.AbilityUsage.AbilityDefinition);
        }
        return (0, null);
    }
}
