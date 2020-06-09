using Mud.Logger;
using Mud.Server.Common;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.POC.Abilities2
{
    public abstract class SpellBase : ISpell
    {
        protected IRandomManager RandomManager { get; }

        protected AbilityInfo AbilityInfo { get; private set; }
        protected ICharacter Caster { get; private set; }
        protected int Level { get; private set; }
        protected int? Cost { get; private set; }
        protected ResourceKinds? ResourceKind { get; private set; }

        protected SpellBase(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        #region ISpell

        public virtual string Setup(AbilityActionInput abilityActionInput)
        {
            // 1) check context
            AbilityInfo = abilityActionInput.AbilityInfo;
            if (AbilityInfo == null)
                return "Internal error: AbilityInfo is null.";
            if (AbilityInfo.AbilityExecutionType != GetType())
                return $"Internal error: AbilityInfo is not of the right type: {AbilityInfo.GetType().Name} instead of {GetType().Name}.";
            // 2) check actor
            Caster = abilityActionInput.Actor as ICharacter;
            if (Caster == null)
                return "Spell must be cast by a character.";
            if (Caster.Room == null)
                return "You are nowhere...";
            Level = abilityActionInput.Level ?? Caster.Level;
            // 3) check targets
            string setTargetResult = SetTargets(abilityActionInput);
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

        public virtual void Execute()
        {
            IPlayableCharacter pcCaster = Caster as IPlayableCharacter;

            // check if failed
            var abilityPercentage = Caster.GetAbilityLearned(AbilityInfo.Name);
            if (!RandomManager.Chance(abilityPercentage.percentage))
            {
                Caster.Send("You lost your concentration.");
                pcCaster?.CheckAbilityImprove(abilityPercentage.abilityLearned, false, 1);
                // pay half resource
                if (Cost.HasValue && Cost.Value > 1 && ResourceKind.HasValue)
                    Caster.UpdateResource(ResourceKind.Value, -Cost.Value / 2);
                return;
            }

            // 6) pay resource
            if (Cost.HasValue && Cost.Value >= 1 && ResourceKind.HasValue)
                Caster.UpdateResource(ResourceKind.Value, -Cost.Value);

            // 7) say spell if not ventriloquate
            SaySpell();

            // 8) invoke spell
            Invoke();

            // 9) GCD
            if (AbilityInfo.PulseWaitTime.HasValue)
                pcCaster?.ImpersonatedBy?.SetGlobalCooldown(AbilityInfo.PulseWaitTime.Value);

            // 10) set cooldown
            if (AbilityInfo.Cooldown.HasValue && AbilityInfo.Cooldown.Value > 0)
                Caster.SetCooldown(AbilityInfo.Name, AbilityInfo.Cooldown.Value);

            // 11) check improve true
            pcCaster?.CheckAbilityImprove(abilityPercentage.abilityLearned, true, AbilityInfo.LearnDifficultyMultiplier);
        }

        #endregion

        protected abstract string SetTargets(AbilityActionInput abilityActionInput);
        protected abstract void Invoke();

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
