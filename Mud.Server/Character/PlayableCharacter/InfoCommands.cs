using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Input;

namespace Mud.Server.Character.PlayableCharacter
{
    public partial class PlayableCharacter
    {
        [PlayableCharacterCommand("affects", "Information")]
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
    }
}
