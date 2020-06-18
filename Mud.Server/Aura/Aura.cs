using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mud.Common;
using Mud.Container;
using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Aura
{
    public class Aura : IAura
    {
        private IAbilityManager AbilityManager => DependencyContainer.Current.GetInstance<IAbilityManager>();
        private IWiznet Wiznet => DependencyContainer.Current.GetInstance<IWiznet>();

        private readonly List<IAffect> _affects;

        private Aura()
        {
            IsValid = true;
        }

        public Aura(string abilityName, IEntity source, AuraFlags flags, int level, TimeSpan duration, params IAffect[] affects)
            : this()
        {
            AbilityName = abilityName;
            Source = source;
            AuraFlags = flags;
            Level = level;
            PulseLeft = Pulse.FromTimeSpan(duration);
            _affects = (affects ?? Enumerable.Empty<IAffect>()).ToList();
        }

        public Aura(AuraData auraData)
            : this()
        {
            // TODO: source
            AbilityName = auraData.AbilityName;
            AuraFlags = auraData.AuraFlags;
            Level = auraData.Level;
            PulseLeft = auraData.PulseLeft;
            _affects = new List<IAffect>();
            if (auraData.Affects != null)
            {
                foreach (AffectDataBase affectData in auraData.Affects)
                {
                    switch (affectData)
                    {
                        case CharacterAttributeAffectData characterAttributeAffectData:
                            _affects.Add(new CharacterAttributeAffect(characterAttributeAffectData));
                            break;
                        case CharacterFlagsAffectData characterFlagsAffectData:
                            _affects.Add(new CharacterFlagsAffect(characterFlagsAffectData));
                            break;
                        case CharacterIRVAffectData characterIRVAffectData:
                            _affects.Add(new CharacterIRVAffect(characterIRVAffectData));
                            break;
                        case CharacterSexAffectData characterSexAffectData:
                            _affects.Add(new CharacterSexAffect(characterSexAffectData));
                            break;
                        case ItemFlagsAffectData itemFlagsAffectData:
                            _affects.Add(new ItemFlagsAffect(itemFlagsAffectData));
                            break;
                        case ItemWeaponFlagsAffectData itemWeaponFlagsAffectData:
                            _affects.Add(new ItemWeaponFlagsAffect(itemWeaponFlagsAffectData));
                            break;
                            // TODO: poison/plague
                        default:
                            Wiznet.Wiznet($"Unexpected AuraAffect type {affectData.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
                            break;
                    }
                }
            }
        }

        #region IAura

        public bool IsValid { get; private set; }

        public int Level { get; private set; }

        public int PulseLeft { get; private set; }

        public string AbilityName { get; private set; }

        public IEntity Source { get; private set; }

        public AuraFlags AuraFlags { get; }

        public IEnumerable<IAffect> Affects => _affects;

        public void Update(int level, TimeSpan duration)
        {
            Level = level;
            PulseLeft = Pulse.FromTimeSpan(duration);
        }

        public T AddOrUpdateAffect<T>(Func<T, bool> filterFunc, Func<T> createFunc, Action<T> updateFunc)
            where T : IAffect
        {
            T affect = _affects.OfType<T>().FirstOrDefault(filterFunc);
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
            Source = null;
        }

        public StringBuilder Append(StringBuilder sb, bool shortDisplay = false)
        {
            // TODO // if lvl < 10: only ability
            // TODO // if lvl < 15: only ability and duration
            // TODO // if lvl >= 15: ability, duration + affects
            // TODO // if admin: ability, level, duration, flags + affects
            // TODO admin see hidden auras

            // TODO: better formatting with spacing like in score
            sb.AppendFormatLine("%B%{0,15}%x% (lvl {1}) {2} {3}",
                    AbilityName.MaxLength(15) ?? "Inherent",
                    Level,
                    AuraFlags.HasFlag(AuraFlags.Permanent)
                        ? "%R%Permanent%x%"
                        : $"%G%{(PulseLeft / Pulse.PulsePerSeconds).FormatDelay()}%x% left",
                    AuraFlags == AuraFlags.None
                        ? ""
                        : AuraFlags.ToString());
            if (!shortDisplay)
                foreach (IAffect affect in Affects)
                {
                    sb.Append("    ");
                    affect.Append(sb);
                    sb.AppendLine();
                }

            return sb;
        }

        public virtual AuraData MapAuraData()
        {
            return new AuraData
            {
                AbilityName = AbilityName,
                Level = Level,
                PulseLeft = PulseLeft,
                AuraFlags = AuraFlags,
                Affects = Affects.Select(x => x.MapAffectData()).ToArray()
            };
        }

        #endregion
    }
}
