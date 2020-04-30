using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;

namespace Mud.Server.Aura
{
    public class Aura : IAura
    {
        private const int NoAbilityId = -1;

        private IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();

        private readonly List<IAffect> _affects;

        public Aura(IAbility ability, IEntity source, AuraFlags flags, int level, TimeSpan ts, params IAffect[] affects)
        {
            IsValid = true;

            Ability = ability;
            Source = source;
            AuraFlags = flags;
            Level = level;
            PulseLeft = Pulse.FromTimeSpan(ts);
            _affects = (affects ?? Enumerable.Empty<IAffect>()).ToList();

            if (ability?.Flags.HasFlag(AbilityFlags.AuraIsHidden) == true)
                AuraFlags |= AuraFlags.Hidden;
        }

        public Aura(AuraData auraData)
        {
            if (auraData.AbilitiId == NoAbilityId)
                Ability = null;
            else
            {
                Ability = AbilityManager.Abilities.FirstOrDefault(x => x.Id == auraData.AbilitiId);
                if (Ability == null)
                    Log.Default.WriteLine(LogLevels.Error, "Aura ability id {0} doesn't exist anymore", auraData.AbilitiId);
            }
            // TODO: source
            AuraFlags = auraData.AuraFlags;
            Level = auraData.Level;
            PulseLeft = auraData.PulseLeft;
            // TODO: other affects
        }

        #region IAura

        public bool IsValid { get; private set; }

        public int Level { get; private set; }

        public int PulseLeft { get; private set; }

        public IAbility Ability { get; private set; }

        public IEntity Source { get; private set; }

        public AuraFlags AuraFlags { get; private set; }

        public IEnumerable<IAffect> Affects => _affects;

        public void Append(StringBuilder sb, bool displayHidden)
        {
            // TODO admin see hidden auras
            // TODO: nicer look like
            //    sb.AppendFormatLine("%B%{0}%x% modifies %W%{1}%x% by %m%{2}{3}%x% for %c%{4}%x%",
            //        aura.Ability == null ? "Unknown" : aura.Ability.Name,
            //        aura.Modifier,
            //        aura.Amount,
            //        aura.AmountOperator == AmountOperators.Fixed ? string.Empty : "%",
            //        StringHelpers.FormatDelay(aura.PulseLeft / Pulse.PulsePerSeconds));
            sb.AppendFormat("{0}(level {1}) duration {2} flags {3} source {4}", Ability?.Name ?? "???", Level, PulseLeft, AuraFlags, Source?.Name ?? "???");
            foreach (IAffect affect in Affects)
            {
                sb.Append("    ");
                affect.Append(sb);
                sb.AppendLine();
            }
        }

        public bool DecreasePulseLeft(int pulseCount)
        {
            throw new NotImplementedException();
        }

        public void OnRemoved()
        {
            IsValid = false;
            Ability = null;
            Source = null;
        }

        public AuraData MapAuraData()
        {
            // TODO
            throw new NotImplementedException();
        }

        #endregion
    }
}
