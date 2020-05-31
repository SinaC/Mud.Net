using System.Linq;
using System.Text;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Helpers;
using Mud.Server.Input;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("affects", "Information")]
        [PlayableCharacterCommand("auras", "Information")]
        protected override CommandExecutionResults DoAffects(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (Auras.Any() || PeriodicAuras.Any())
            {
                sb.AppendLine("%c%You are affected by following auras:%x%");
                // Auras
                foreach (IAura aura in Auras.Where(x => !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.PulseLeft))
                    aura.Append(sb);
                // TODO
                //// Periodic auras
                //foreach (IPeriodicAura pa in _periodicAuras.Where(x => x.Ability == null || (x.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden))
                //{
                //    if (pa.AuraType == PeriodicAuraTypes.Damage)
                //        sb.AppendFormatLine("%B%{0}%x% %W%deals {1}{2}%x% {3} damage every %g%{4}%x% for %c%{5}%x%",
                //            pa.Ability == null ? "Unknown" : pa.Ability.Name,
                //            pa.Amount,
                //            pa.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
                //            StringHelpers.SchoolTypeColor(pa.School),
                //            StringHelpers.FormatDelay(pa.TickDelay),
                //            StringHelpers.FormatDelay(pa.SecondsLeft));
                //    else
                //        sb.AppendFormatLine("%B%{0}%x% %W%heals {1}{2}%x% hp every %g%{3}%x% for %c%{4}%x%",
                //            pa.Ability == null ? "Unknown" : pa.Ability.Name,
                //            pa.Amount,
                //            pa.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
                //            StringHelpers.FormatDelay(pa.TickDelay),
                //            StringHelpers.FormatDelay(pa.SecondsLeft));
                //}
            }
            else
                sb.AppendLine("%c%You are not affected by any spells.%x%");

            if (Pets.Any())
                foreach (INonPlayableCharacter pet in Pets.Where(x => x.Auras.Any()))
                {
                    sb.AppendFormatLine("%c%{0} is affected by following auras:%x%", pet.DisplayName);
                    foreach (IAura aura in pet.Auras.Where(x => !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.PulseLeft))
                        aura.Append(sb);
                }

            Page(sb);
            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("saffects", "Information")]
        [PlayableCharacterCommand("sauras", "Information")]
        protected override CommandExecutionResults DoShortAffects(string rawParameters, params CommandParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (Auras.Any() || PeriodicAuras.Any())
            {
                sb.AppendLine("%c%You are affected by the following auras:%x%");
                // Auras
                foreach (IAura aura in Auras.Where(x => !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.PulseLeft))
                    aura.Append(sb, true);
                // TODO
                //// Periodic auras
                //foreach (IPeriodicAura pa in _periodicAuras.Where(x => x.Ability == null || (x.Ability.Flags & AbilityFlags.AuraIsHidden) != AbilityFlags.AuraIsHidden))
                //{
                //    if (pa.AuraType == PeriodicAuraTypes.Damage)
                //        sb.AppendFormatLine("%B%{0}%x% %W%deals {1}{2}%x% {3} damage every %g%{4}%x% for %c%{5}%x%",
                //            pa.Ability == null ? "Unknown" : pa.Ability.Name,
                //            pa.Amount,
                //            pa.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
                //            StringHelpers.SchoolTypeColor(pa.School),
                //            StringHelpers.FormatDelay(pa.TickDelay),
                //            StringHelpers.FormatDelay(pa.SecondsLeft));
                //    else
                //        sb.AppendFormatLine("%B%{0}%x% %W%heals {1}{2}%x% hp every %g%{3}%x% for %c%{4}%x%",
                //            pa.Ability == null ? "Unknown" : pa.Ability.Name,
                //            pa.Amount,
                //            pa.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
                //            StringHelpers.FormatDelay(pa.TickDelay),
                //            StringHelpers.FormatDelay(pa.SecondsLeft));
                //}
            }
            else
                sb.AppendLine("%c%You are not affected by any spells.%x%");

            if (Pets.Any())
                foreach (INonPlayableCharacter pet in Pets.Where(x => x.Auras.Any()))
                {
                    sb.AppendFormatLine("%c%{0} is affected by following auras:%x%", pet.DisplayName);
                    foreach (IAura aura in pet.Auras.Where(x => !x.AuraFlags.HasFlag(AuraFlags.Hidden)).OrderBy(x => x.PulseLeft))
                        aura.Append(sb, true);
                }

            Page(sb);
            return CommandExecutionResults.Ok;
        }

        [PlayableCharacterCommand("auto", "Information")]
        protected virtual CommandExecutionResults DoAuto(string rawParameters, params CommandParameter[] parameters)
        {
            if (parameters.Length == 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (AutoFlags autoFlag in EnumHelpers.GetValues<AutoFlags>().Where(x => x != AutoFlags.None).OrderBy(x => x.ToString()))
                    sb.AppendFormatLine("{0}: {1}", autoFlag.PrettyPrint(), AutoFlags.HasFlag(autoFlag) ? "ON" : "OFF");

                Send(sb);
                return CommandExecutionResults.Ok;
            }

            if (parameters[0].IsAll)
            {
                foreach (AutoFlags autoFlag in EnumHelpers.GetValues<AutoFlags>().Where(x => x != AutoFlags.None).OrderBy(x => x.ToString()))
                    AutoFlags |= autoFlag;
                Send("Ok.");
                return CommandExecutionResults.Ok;
            }

            bool found = EnumHelpers.TryFindByPrefix(parameters[0].Value, out var flag, AutoFlags.None);
            if (!found)
            {
                Send("This is not a valid auto.");
                return CommandExecutionResults.SyntaxErrorNoDisplay;
            }

            if (AutoFlags.HasFlag(flag))
            {
                AutoFlags &= ~flag;
                string msg = AutoRemovedMessage(flag);
                Send(msg);
            }
            else
            {
                AutoFlags |= flag;
                string msg = AutoAddedMessage(flag);
                Send(msg);
            }

            return CommandExecutionResults.Ok;
        }

        //
        private string AutoAddedMessage(AutoFlags flag)
        {
            switch (flag)
            {
                case AutoFlags.Assist: return "You will now assist when needed.";
                case AutoFlags.Exit: return "Exits will now be displayed.";
                case AutoFlags.Sacrifice: return "Automatic corpse sacrificing set.";
                case AutoFlags.Gold: return "Automatic gold looting set.";
                case AutoFlags.Loot: return "Automatic corpse looting set.";
                case AutoFlags.Split: return "Automatic gold splitting set.";
                default:
                    Wiznet.Wiznet($"AutoAddedMessage: invalid flag {flag}", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return "???";
            }
        }

        private string AutoRemovedMessage(AutoFlags flag)
        {
            switch (flag)
            {
                case AutoFlags.Assist:return "Autoassist removed.";
                case AutoFlags.Exit: return "Exits will no longer be displayed.";
                case AutoFlags.Sacrifice: return "Autosacrificing removed.";
                case AutoFlags.Gold: return "Autogold removed.";
                case AutoFlags.Loot: return "Autolooting removed.";
                case AutoFlags.Split: return "Autosplitting removed.";
                default:
                    Wiznet.Wiznet($"AutoRemovedMessage: invalid flag {flag}", WiznetFlags.Bugs, AdminLevels.Implementor);
                    return "???";
            }
        }
    }
}
