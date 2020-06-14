using Mud.Logger;
using Mud.Server.Random;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using Mud.Server.Common;

namespace Mud.POC.Abilities2
{
    public abstract class SpellBase : ISpell
    {
        protected IRandomManager RandomManager { get; }

        protected bool IsSetupExecuted { get; private set; }
        protected IAbilityInfo AbilityInfo { get; private set; }
        protected ICharacter Caster { get; private set; }
        protected int Level { get; private set; }
        protected int? Cost { get; private set; }
        protected ResourceKinds? ResourceKind { get; private set; }
        protected bool IsCastFromItem { get; private set; }

        protected SpellBase(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        #region ISpell

        public virtual string Setup(ISpellActionInput spellActionInput)
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

        protected abstract string SetTargets(ISpellActionInput spellActionInput);
        protected abstract void Invoke();

        private string SetupFromCast(ISpellActionInput spellActionInput)
        {
            // 1) check context
            AbilityInfo = spellActionInput.AbilityInfo;
            if (AbilityInfo == null)
                return "Internal error: AbilityInfo is null.";
            if (AbilityInfo.AbilityExecutionType != GetType())
                return $"Internal error: AbilityInfo is not of the right type: {AbilityInfo.GetType().Name} instead of {GetType().Name}.";
            // 2) check caster
            Caster = spellActionInput.Caster;
            if (Caster == null)
                return "Spell must be cast by a character.";
            if (Caster.Room == null)
                return "You are nowhere...";
            Level = spellActionInput.Level;
            // 3) check targets
            string setTargetResult = SetTargets(spellActionInput);
            if (setTargetResult != null)
                return setTargetResult;
            // 4) check cooldown
            int cooldownPulseLeft = Caster.CooldownPulseLeft(AbilityInfo.Name);
            if (cooldownPulseLeft > 0)
                return $"{AbilityInfo.Name} is in cooldown for {StringHelpers.FormatDelay(cooldownPulseLeft / Pulse.PulsePerSeconds)}.";
            // 5) check resource cost
            var abilityPercentage = Caster.GetAbilityLearned(AbilityInfo.Name);
            if (abilityPercentage.abilityLearned.ResourceKind.HasValue && abilityPercentage.abilityLearned.CostAmount > 0 && abilityPercentage.abilityLearned.CostAmountOperator != CostAmountOperators.None)
            {
                ResourceKinds resourceKind = abilityPercentage.abilityLearned.ResourceKind.Value;
                if (!Caster.CurrentResourceKinds.Contains(resourceKind)) // TODO: not sure about this test
                    return $"You can't use {resourceKind} as resource for the moment.";
                int resourceLeft = Caster[resourceKind];
                int cost;
                switch (abilityPercentage.abilityLearned.CostAmountOperator)
                {
                    case CostAmountOperators.Fixed:
                        cost = abilityPercentage.abilityLearned.CostAmount;
                        break;
                    case CostAmountOperators.Percentage:
                        cost = Caster.MaxResource(resourceKind) * abilityPercentage.abilityLearned.CostAmount / 100;
                        break;
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "Unexpected CostAmountOperator {0} for ability {1}.", abilityPercentage.abilityLearned.CostAmountOperator, AbilityInfo.Name);
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

        private string SetupFromItem(ISpellActionInput spellActionInput)
        {
            // 1) check context
            AbilityInfo = spellActionInput.AbilityInfo;
            if (AbilityInfo == null)
                return "Internal error: AbilityInfo is null.";
            if (AbilityInfo.AbilityExecutionType != GetType())
                return $"Internal error: AbilityInfo is not of the right type: {AbilityInfo.GetType().Name} instead of {GetType().Name}.";
            // 2) check caster
            Caster = spellActionInput.Caster;
            if (Caster == null)
                return "Spell must be cast by a character.";
            if (Caster.Room == null)
                return "You are nowhere...";
            Level = spellActionInput.Level;
            // 3) check targets
            string setTargetResult = SetTargets(spellActionInput);
            if (setTargetResult != null)
                return setTargetResult;
            return null;
        }

        private void ExecuteFromCast()
        {
            IPlayableCharacter pcCaster = Caster as IPlayableCharacter;

            // 1) check if failed
            var abilityLearned = Caster.GetAbilityLearned(AbilityInfo.Name);
            if (!RandomManager.Chance(abilityLearned.percentage))
            {
                Caster.Send("You lost your concentration.");
                pcCaster?.CheckAbilityImprove(abilityLearned.abilityLearned, false, 1);
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
            if (AbilityInfo.PulseWaitTime.HasValue)
                pcCaster?.ImpersonatedBy?.SetGlobalCooldown(AbilityInfo.PulseWaitTime.Value);

            // 6) set cooldown if any
            if (AbilityInfo.Cooldown.HasValue && AbilityInfo.Cooldown.Value > 0)
                Caster.SetCooldown(AbilityInfo.Name, AbilityInfo.Cooldown.Value);

            // 7) check improve true
            pcCaster?.CheckAbilityImprove(abilityLearned.abilityLearned, true, AbilityInfo.LearnDifficultyMultiplier);
        }

        private void ExecuteFromItem()
        {
            Invoke();
        }

        private static readonly Dictionary<string, string> SyllableTable = new Dictionary<string, string> // TODO: use Trie ?
        {
            { " ",      " "     },
            { "ar",     "abra"      },
            { "au",     "kada"      },
            { "bless",  "fido"      },
            { "blind",  "nose"      },
            { "bur",    "mosa"      },
            { "cu",     "judi"      },
            { "de",     "oculo"     },
            { "en",     "unso"      },
            { "light",  "dies"      },
            { "lo",     "hi"        },
            { "mor",    "zak"       },
            { "move",   "sido"      },
            { "ness",   "lacri"     },
            { "ning",   "illa"      },
            { "per",    "duda"      },
            { "ra",     "gru"       },
            { "fresh",  "ima"       },
            { "re",     "candus"    },
            { "son",    "sabru"     },
            { "tect",   "infra"     },
            { "tri",    "cula"      },
            { "ven",    "nofo"      },
            { "a", "a" }, { "b", "b" }, { "c", "q" }, { "d", "e" },
            { "e", "z" }, { "f", "y" }, { "g", "o" }, { "h", "p" },
            { "i", "u" }, { "j", "y" }, { "k", "t" }, { "l", "r" },
            { "m", "w" }, { "n", "i" }, { "o", "a" }, { "p", "s" },
            { "q", "d" }, { "r", "f" }, { "s", "g" }, { "t", "h" },
            { "u", "j" }, { "v", "z" }, { "w", "x" }, { "x", "n" },
            { "y", "l" }, { "z", "k" }
        };

        // TODO: maybe a table should be constructed for each spell to avoid computing at each cast
        protected virtual void SaySpell()
        {
            //source.Send("You cast '{0}'.", ability.Name);

            // Build mystical words for spell
            StringBuilder mysticalWords = new StringBuilder();
            string abilityName = AbilityInfo.Name.ToLowerInvariant();
            string remaining = abilityName;
            while (remaining.Length > 0)
            {
                bool found = false;
                foreach (var syllable in SyllableTable)
                {
                    if (remaining.StartsWith(syllable.Key))
                    {
                        mysticalWords.Append(syllable.Value);
                        remaining = remaining.Substring(syllable.Key.Length);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    mysticalWords.Append('?');
                    remaining = remaining.Substring(1);
                    Log.Default.WriteLine(LogLevels.Warning, "Spell {0} contains a character which is not found in syllable table", AbilityInfo.Name);
                }
            }

            // Say to people in room except source
            foreach (ICharacter target in Caster.Room.People.Where(x => x != Caster))
            {
                if (target.LearnedAbilities.Any(x => x.Name == AbilityInfo.Name && x.Level < target.Level))
                    target.Act(ActOptions.ToCharacter, "{0} casts the spell '{1}'.", Caster, AbilityInfo.Name);
                else
                {

                    target.Act(ActOptions.ToCharacter, "{0} utters the words, '{1}'.", Caster, mysticalWords);
                }
            }
        }
    }
}
