using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System.Text;

namespace Mud.Server.Ability.Spell;

public abstract class SpellBase : ISpell
{
    protected ILogger<SpellBase> Logger { get; }
    protected IRandomManager RandomManager { get; }

    protected bool IsSetupExecuted { get; private set; }
    protected IAbilityDefinition AbilityDefinition { get; private set; } = default!;
    protected ICharacter Caster { get; private set; } = default!;
    protected int Level { get; private set; }
    protected int? Cost { get; private set; }
    protected ResourceKinds? ResourceKind { get; private set; }
    protected bool IsCastFromItem { get; private set; }

    protected SpellBase(ILogger<SpellBase> logger, IRandomManager randomManager)
    {
        Logger = logger;
        RandomManager = randomManager;
    }

    #region ISpell

    public virtual string? Setup(ISpellActionInput spellActionInput)
    {
        IsSetupExecuted = true;

        IsCastFromItem = spellActionInput.IsCastFromItem;
        if (IsCastFromItem)
            return SetupFromItem(spellActionInput);
        else
            return SetupFromCast(spellActionInput);
    }

    public virtual void Execute()
    {
        if (!IsSetupExecuted)
            throw new Exception("Cannot execute spell without calling setup first.");
        if (IsCastFromItem)
            ExecuteFromItem();
        else
            ExecuteFromCast();
    }

    #endregion

    protected abstract string? SetTargets(ISpellActionInput spellActionInput);
    protected abstract void Invoke();

    private string? SetupFromCast(ISpellActionInput spellActionInput)
    {
        // 1) check context
        AbilityDefinition = spellActionInput.AbilityDefinition;
        if (AbilityDefinition == null)
            return "Internal error: AbilityDefinition is null.";
        if (AbilityDefinition.AbilityExecutionType != GetType())
            return $"Internal error: AbilityDefinition is not of the right type: {AbilityDefinition.GetType().Name} instead of {GetType().Name}.";

        // 2) check caster
        Caster = spellActionInput.Caster;
        if (Caster == null)
            return "Spell must be cast by a character.";
        if (Caster.Room == null)
            return "You are nowhere...";
        Level = spellActionInput.Level;

        // 3) check targets
        var setTargetResult = SetTargets(spellActionInput);
        if (setTargetResult != null)
            return setTargetResult;

        // 4) check cooldown
        int cooldownPulseLeft = Caster.CooldownPulseLeft(AbilityDefinition.Name);
        if (cooldownPulseLeft > 0)
            return $"{AbilityDefinition.Name} is in cooldown for {(cooldownPulseLeft / Pulse.PulsePerSeconds).FormatDelay()}.";

        // 5) check resource cost
        var (_, abilityLearned) = Caster.GetAbilityLearnedAndPercentage(AbilityDefinition.Name);
        if (abilityLearned != null && abilityLearned.ResourceKind.HasValue && abilityLearned.CostAmount > 0 && abilityLearned.CostAmountOperator != CostAmountOperators.None)
        {
            var resourceKind = abilityLearned.ResourceKind.Value;
            if (!Caster.CurrentResourceKinds.Contains(resourceKind)) // TODO: not sure about this test
                return $"You can't use {resourceKind} as resource for the moment.";
            int resourceLeft = Caster[resourceKind];
            int cost;
            switch (abilityLearned.CostAmountOperator)
            {
                case CostAmountOperators.Fixed:
                    cost = abilityLearned.CostAmount;
                    break;
                case CostAmountOperators.Percentage:
                    cost = Caster.MaxResource(resourceKind) * abilityLearned.CostAmount / 100;
                    break;
                default:
                    Logger.LogError("Unexpected CostAmountOperator {costAmountOperator} for ability {abilityName}.", abilityLearned.CostAmountOperator, AbilityDefinition.Name);
                    cost = 100;
                    break;
            }
            bool enoughResource = cost <= resourceLeft;
            if (!enoughResource)
                return $"You don't have enough {resourceKind}.";
            Cost = cost;
            ResourceKind = resourceKind;
        }
        else
        {
            Cost = null;
            ResourceKind = null;
        }
        return null;
    }

    private string? SetupFromItem(ISpellActionInput spellActionInput)
    {
        // 1) check context
        AbilityDefinition = spellActionInput.AbilityDefinition;
        if (AbilityDefinition == null)
            return "Internal error: AbilityDefinition is null.";
        if (AbilityDefinition.AbilityExecutionType != GetType())
            return $"Internal error: AbilityDefinition is not of the right type: {AbilityDefinition.GetType().Name} instead of {GetType().Name}.";

        // 2) check caster
        Caster = spellActionInput.Caster;
        if (Caster == null)
            return "Spell must be cast by a character.";
        if (Caster.Room == null)
            return "You are nowhere...";
        Level = spellActionInput.Level;

        // 3) check targets
        var setTargetResult = SetTargets(spellActionInput);
        if (setTargetResult != null)
            return setTargetResult;
        return null;
    }

    private void ExecuteFromCast()
    {
        var pcCaster = Caster as IPlayableCharacter;

        // 1) check if failed
        var abilityLearned = Caster.GetAbilityLearnedAndPercentage(AbilityDefinition.Name);
        if (!RandomManager.Chance(abilityLearned.percentage))
        {
            Caster.Send("You lost your concentration.");
            pcCaster?.CheckAbilityImprove(AbilityDefinition.Name, false, 1);
            // pay half resource
            if (Cost.HasValue && Cost.Value > 1 && ResourceKind.HasValue)
                Caster.UpdateResource(ResourceKind.Value, -Cost.Value / 2);
            return;
        }

        // 2) pay resource
        if (Cost.HasValue && Cost.Value >= 1 && ResourceKind.HasValue)
            Caster.UpdateResource(ResourceKind.Value, -Cost.Value);

        // 3) say spell if not ventriloquate
        SaySpell();

        // 4) invoke spell
        Invoke();

        // 5) GCD
        if (AbilityDefinition.PulseWaitTime.HasValue)
            pcCaster?.ImpersonatedBy?.SetGlobalCooldown(AbilityDefinition.PulseWaitTime.Value);

        // 6) set cooldown if any
        if (AbilityDefinition.CooldownInSeconds.HasValue && AbilityDefinition.CooldownInSeconds.Value > 0)
            Caster.SetCooldown(AbilityDefinition.Name, TimeSpan.FromSeconds(AbilityDefinition.CooldownInSeconds.Value));

        // 7) check improve true
        pcCaster?.CheckAbilityImprove(AbilityDefinition.Name, true, AbilityDefinition.LearnDifficultyMultiplier);
    }

    private void ExecuteFromItem()
    {
        Invoke();
    }

    private static readonly (string syllable, string transformed)[] SyllableTable = 
    [
        ( " ",      " "         ),
        ( "ar",     "abra"      ),
        ( "au",     "kada"      ),
        ( "bless",  "fido"      ),
        ( "blind",  "nose"      ),
        ( "bur",    "mosa"      ),
        ( "cu",     "judi"      ),
        ( "de",     "oculo"     ),
        ( "en",     "unso"      ),
        ( "light",  "dies"      ),
        ( "lo",     "hi"        ),
        ( "mor",    "zak"       ),
        ( "move",   "sido"      ),
        ( "ness",   "lacri"     ),
        ( "ning",   "illa"      ),
        ( "per",    "duda"      ),
        ( "ra",     "gru"       ),
        ( "fresh",  "ima"       ),
        ( "re",     "candus"    ),
        ( "son",    "sabru"     ),
        ( "tect",   "infra"     ),
        ( "tri",    "cula"      ),
        ( "ven",    "nofo"      ),
        ( "a", "a" ), ( "b", "b" ), ( "c", "q" ), ( "d", "e" ),
        ( "e", "z" ), ( "f", "y" ), ( "g", "o" ), ( "h", "p" ),
        ( "i", "u" ), ( "j", "y" ), ( "k", "t" ), ( "l", "r" ),
        ( "m", "w" ), ( "n", "i" ), ( "o", "a" ), ( "p", "s" ),
        ( "q", "d" ), ( "r", "f" ), ( "s", "g" ), ( "t", "h" ),
        ( "u", "j" ), ( "v", "z" ), ( "w", "x" ), ( "x", "n" ),
        ( "y", "l" ), ( "z", "k" )
    ];

    // TODO: maybe a table should be constructed for each spell to avoid computing at each cast
    protected virtual void SaySpell()
    {
        //source.Send("You cast '{0}'.", ability.Name);

        // Build mystical words for spell
        StringBuilder mysticalWords = new();
        string abilityName = AbilityDefinition.Name.ToLowerInvariant();
        string remaining = abilityName;
        while (remaining.Length > 0)
        {
            bool found = false;
            foreach (var syllable in SyllableTable)
            {
                if (remaining.StartsWith(syllable.syllable))
                {
                    mysticalWords.Append(syllable.transformed);
                    remaining = remaining[syllable.syllable.Length..];
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                mysticalWords.Append('?');
                remaining = remaining[1..];
                Logger.LogWarning("Spell {abilityName} contains a character which is not found in syllable table", AbilityDefinition.Name);
            }
        }

        // Say to people in room except source
        foreach (ICharacter target in Caster.Room.People.Where(x => x != Caster))
        {
            var (_, abilityLearned) = target.GetAbilityLearnedAndPercentage(AbilityDefinition.Name);
            if (abilityLearned != null && abilityLearned.Level < target.Level)
                target.Act(ActOptions.ToCharacter, "{0} casts the spell '{1}'.", Caster, AbilityDefinition.Name); // known the spell
            else
                target.Act(ActOptions.ToCharacter, "{0} utters the words, '{1}'.", Caster, mysticalWords); // doesn't known the spell
        }
    }
}
