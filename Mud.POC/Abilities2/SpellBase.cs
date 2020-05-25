using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Input;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.POC.Abilities2.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mud.POC.Abilities2
{
    public abstract class SpellBase : ISpell
    {
        protected IRandomManager RandomManager { get; }
        protected IWiznet Wiznet { get; }

        protected SpellBase(IRandomManager randomManager, IWiznet wiznet)
        {
            RandomManager = randomManager;
            Wiznet = wiznet;
        }

        #region IAbility

        public abstract int Id { get; }

        public abstract string Name { get; }

        public virtual int PulseWaitTime => 12;

        public virtual int Cooldown => 0;

        public virtual int LearnDifficultyMultiplier => 1;

        public virtual AbilityFlags Flags => AbilityFlags.None;

        public abstract AbilityEffects Effects { get; }

        public virtual CastResults Cast(ICharacter caster, KnownAbility knownAbility, string rawParameters, params CommandParameter[] parameters)
        {
            IPlayableCharacter pcCaster = caster as IPlayableCharacter;

            // 1) set targets if any
            AbilityTargetResults targetResult = SetTargets(caster, rawParameters, parameters);
            if (targetResult != AbilityTargetResults.Ok)
                return MapCastResultToCommandExecutionResult(targetResult);

            // 2) check cooldown
            int cooldownPulseLeft = caster.CooldownPulseLeft(knownAbility.Ability);
            if (cooldownPulseLeft > 0)
            {
                caster.Send("{0} is in cooldown for {1}.", knownAbility.Ability.Name, StringHelpers.FormatDelay(cooldownPulseLeft / Pulse.PulsePerSeconds));
                return CastResults.InCooldown;
            }

            // 3) check resources
            int? cost = null;
            if (knownAbility.ResourceKind.HasValue && knownAbility.CostAmount > 0 && knownAbility.CostAmountOperator != CostAmountOperators.None)
            {
                ResourceKinds resourceKind = knownAbility.ResourceKind.Value;
                if (!caster.CurrentResourceKinds.Contains(resourceKind)) // TODO: not sure about this test
                {
                    caster.Send("You can't use {0} as resource for the moment.", knownAbility.ResourceKind);
                    return CastResults.CantUseRequiredResource;
                }
                int resourceLeft = caster[resourceKind];
                switch (knownAbility.CostAmountOperator)
                {
                    case CostAmountOperators.Fixed:
                        cost = knownAbility.CostAmount;
                        break;
                    case CostAmountOperators.Percentage:
                        cost = caster.MaxResource(resourceKind) * knownAbility.CostAmount / 100;
                        break;
                    default:
                        Wiznet.Wiznet($"Unexpected CostAmountOperator {knownAbility.CostAmountOperator} for ability {knownAbility.Ability.Name}", WiznetFlags.Bugs, AdminLevels.Implementor);
                        cost = 100;
                        break;
                }
                bool enoughResource = cost <= resourceLeft;
                if (!enoughResource)
                {
                    caster.Send("You don't have enough {0}.", knownAbility.ResourceKind);
                    return CastResults.NotEnoughResource;
                }
            }

            // 4) check if failed
            var abilityLearnInfo = caster.GetLearnInfo(knownAbility.Ability);
            if (!RandomManager.Chance(abilityLearnInfo.learned))
            {
                caster.Send("You lost your concentration.");
                pcCaster?.CheckAbilityImprove(knownAbility, false, 1);
                // pay half resource
                if (cost.HasValue && cost.Value > 1)
                    caster.UpdateResource(knownAbility.ResourceKind.Value, -cost.Value / 2);
                return CastResults.Failed;
            }

            // 5) pay resource
            if (cost.HasValue && cost.Value >= 1)
                caster.UpdateResource(knownAbility.ResourceKind.Value, -cost.Value);

            // 6) say spell if not ventriloquate
            if (!StringCompareHelpers.StringEquals(knownAbility.Ability.Name, "ventriloquate")) // TODO: override in SpellVentriloquate
                SaySpell(knownAbility.Ability, caster);

            // 7) invoke spell
            Invoke(caster, caster.Level, rawParameters, parameters);

            // 8) GCD
            pcCaster?.ImpersonatedBy?.SetGlobalCooldown(knownAbility.Ability.PulseWaitTime);

            // 8) set cooldown
            if (knownAbility.Ability.Cooldown > 0)
                caster.SetCooldown(knownAbility.Ability);

            // 10) check improve true
            pcCaster?.CheckAbilityImprove(knownAbility, true, knownAbility.Ability.LearnDifficultyMultiplier);

            //
            return CastResults.Ok;
        }

        #endregion

        protected abstract AbilityTargetResults SetTargets(ICharacter caster, string rawParameters, params CommandParameter[] parameters);
        protected abstract void Invoke(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters);

        protected bool SavesDispel(int dispelLevel, int spellLevel, int pulseLeft)
        {
            if (pulseLeft < 0) // very hard to dispel permanent effects
                spellLevel += 5;

            int save = 50 + (spellLevel - dispelLevel) * 5;
            save = save.Range(5, 95);
            return RandomManager.Chance(save);
        }

        protected bool SavesDispel(int dispelLevel, IAura aura)
        {
            if (aura.AuraFlags.HasFlag(AuraFlags.NoDispel))
                return true;
            int auraLevel = aura.Level;
            if (aura.AuraFlags.HasFlag(AuraFlags.Permanent)
                || aura.PulseLeft < 0) // very hard to dispel permanent effects
                auraLevel += 5;

            int save = 50 + (auraLevel - dispelLevel) * 5;
            save = save.Range(5, 95);
            return RandomManager.Chance(save);
        }

        private CastResults MapCastResultToCommandExecutionResult(AbilityTargetResults result)
        {
            switch (result)
            {
                case AbilityTargetResults.MissingParameter:
                    return CastResults.MissingParameter;
                case AbilityTargetResults.InvalidTarget:
                    return CastResults.InvalidTarget;
                case AbilityTargetResults.TargetNotFound:
                    return CastResults.TargetNotFound;
                case AbilityTargetResults.Error:
                    return CastResults.Error;
                default:
                    Wiznet.Wiznet($"Unexpected AbilityTargetResults {result}", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return CastResults.Error;
            }
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
        protected virtual void SaySpell(IAbility ability, ICharacter source)
        {
            if (ability == null)
                return;
            //source.Send("You cast '{0}'.", ability.Name);

            // Build mystical words for spell
            StringBuilder mysticalWords = new StringBuilder();
            string abilityName = ability.Name.ToLowerInvariant();
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
                    Log.Default.WriteLine(LogLevels.Warning, "Spell {0} contains a character which is not found in syllable table", ability.Name);
                }
            }

            // Say to people in room except source
            foreach (ICharacter target in source.Room.People.Where(x => x != source))
            {
                if (target.KnownAbilities.Any(x => x.Ability == ability && x.Level < target.Level))
                    target.Act(ActOptions.ToCharacter, "{0} casts the spell '{1}'.", source, ability.Name);
                else
                {

                    target.Act(ActOptions.ToCharacter, "{0} utters the words, '{1}'.", source, mysticalWords);
                }
            }
        }
    }
}
