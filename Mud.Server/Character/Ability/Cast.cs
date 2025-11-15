using Mud.Domain;
using Mud.Logger;
using Mud.Server.Ability.Spell;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Ability;

[CharacterCommand("cast", "Ability", Priority = 2, MinPosition = Positions.Standing)]
[Syntax("[cmd] <ability> <target>")]
public class Cast : CharacterGameAction
{
    private IAbilityManager AbilityManager { get; }

    public Cast(IAbilityManager abilityManager)
    {
        AbilityManager = abilityManager;
    }

    protected ISpell SpellInstance { get; set; } = default!;

    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        var spellName = actionInput.Parameters.Length > 0 ? actionInput.Parameters[0].Value : null;
        if (string.IsNullOrWhiteSpace(spellName))
            return "Cast what ?";
        var abilityInfo = AbilityManager.Search(spellName, AbilityTypes.Spell);
        if (abilityInfo == null)
            return "This spell doesn't exist.";
        var abilityLearned = Actor.GetAbilityLearned(abilityInfo.Name);
        if (abilityLearned == null)
            return "You don't know any spells of that name.";
        SpellInstance = AbilityManager.CreateInstance<ISpell>(abilityInfo.Name)!;
        if (SpellInstance == null)
        {
            Log.Default.WriteLine(LogLevels.Error, "Spell {0} cannot be created.", abilityInfo.Name);
            return "Something goes wrong.";
        }

        var newParameters = actionInput.Parameters.Skip(1).ToArray();
        var spellActionInput = new SpellActionInput(abilityInfo, Actor, Actor.Level, newParameters);
        var spellInstanceGuards = SpellInstance.Setup(spellActionInput);
        return spellInstanceGuards;
    }

    public override void Execute(IActionInput actionInput)
    {
        SpellInstance.Execute();
    }
}
