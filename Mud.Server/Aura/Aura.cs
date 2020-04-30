using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Helpers;

namespace Mud.Server.Aura
{
    public class Aura : IAura
    {
        private const int NoAbilityId = -1;

        private IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();

        private readonly List<IAffect> _affects;

        private Aura()
        {
            IsValid = true;
        }

        public Aura(IAbility ability, IEntity source, AuraFlags flags, int level, TimeSpan ts, params IAffect[] affects)
            : this()
        {
            Ability = ability;
            Source = source;
            AuraFlags = flags;
            Level = level;
            PulseLeft = Pulse.FromTimeSpan(ts);
            if (ability?.Flags.HasFlag(AbilityFlags.AuraIsHidden) == true)
                AuraFlags |= AuraFlags.Hidden;

            _affects = (affects ?? Enumerable.Empty<IAffect>()).ToList();
        }

        public Aura(AuraData auraData)
            : this()
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
            // TODO: affects
            _affects = new List<IAffect>();
        }

        #region IAura

        public bool IsValid { get; private set; }

        public int Level { get; private set; }

        public int PulseLeft { get; private set; }

        public IAbility Ability { get; private set; }

        public IEntity Source { get; private set; }

        public AuraFlags AuraFlags { get; private set; }

        public IEnumerable<IAffect> Affects => _affects;

        public T AddOrUpdate<T>(Func<T, bool> filterFunc, Func<T> createFunc, Action<T> updateFunc)
            where T : IAffect
        {
            T affect = _affects.OfType<T>().FirstOrDefault(x => filterFunc(x));
            if (affect == null)
            {
                if (createFunc != null)
                {
                    affect = createFunc();
                    _affects.Add(affect);
                }
            }
            else
                updateFunc?.Invoke(affect);
            return affect;
        }

        public bool DecreasePulseLeft(int pulseCount)
        {
            if (AuraFlags.HasFlag(AuraFlags.Permanent) || PulseLeft < 0)
                return false;
            PulseLeft = Math.Max(PulseLeft - pulseCount, 0);
            return PulseLeft == 0;
        }

        public void DecreaseLevel()
        {
            Level = Math.Max(1, Level - 1);
        }

        public void OnRemoved()
        {
            IsValid = false;
            Ability = null;
            Source = null;
        }

        public void Append(StringBuilder sb)
        {
            // TODO // if lvl < 10: only ability
            // TODO // if lvl < 15: only ability and duration
            // TODO // if lvl >= 15: ability, duration + affects
            // TODO // if admin: ability, level, duration, flags + affects
            // TODO admin see hidden auras

            // TODO: better formatting with spacing like in score
            sb.AppendFormatLine("%B%{0}%x% (lvl {1}) {2} left {3}",
                    Ability?.Name ?? "Inherent",
                    Level,
                    AuraFlags.HasFlag(AuraFlags.Permanent)
                        ? "%r%Permanent%x%"
                        : $"%y%{StringHelpers.FormatDelay(PulseLeft / Pulse.PulsePerSeconds)}%x%",
                    AuraFlags == AuraFlags.None
                        ? ""
                        : AuraFlags.ToString());
            foreach (IAffect affect in Affects)
            {
                sb.Append("    ");
                affect.Append(sb);
                sb.AppendLine();
            }
        }

        public AuraData MapAuraData()
        {
            // TODO
            throw new NotImplementedException();
        }

        #endregion
    }
}
