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
        private IWiznet Wiznet => DependencyContainer.Current.GetInstance<IWiznet>();

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
            _affects = (affects ?? Enumerable.Empty<IAffect>()).ToList();
        }

        public Aura(AuraData auraData)
            : this()
        {
            if (auraData.AbilityId == NoAbilityId)
                Ability = null;
            else
            {
                Ability = AbilityManager[auraData.AbilityId];
                if (Ability == null)
                    Log.Default.WriteLine(LogLevels.Error, "Aura ability id {0} doesn't exist anymore", auraData.AbilityId);
            }
            // TODO: source
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
                        default:
                            string msg = $"Unexpected AuraAffect type {affectData.GetType()}";
                            Wiznet.Wiznet(msg, WiznetFlags.Bugs, AdminLevels.Implementor);
                            Log.Default.WriteLine(LogLevels.Error, msg);
                            break;
                    }
                }
            }
        }

        #region IAura

        public bool IsValid { get; private set; }

        public int Level { get; private set; }

        public int PulseLeft { get; private set; }

        public IAbility Ability { get; private set; }

        public IEntity Source { get; private set; }

        public AuraFlags AuraFlags { get; private set; }

        public IEnumerable<IAffect> Affects => _affects;

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
            sb.AppendFormatLine("%B%{0}%x% (lvl {1}) {2} {3}",
                    Ability?.Name ?? "Inherent",
                    Level,
                    AuraFlags.HasFlag(AuraFlags.Permanent)
                        ? "%R%Permanent%x%"
                        : $"%G%{StringHelpers.FormatDelay(PulseLeft / Pulse.PulsePerSeconds)}%x% left",
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

        public virtual AuraData MapAuraData()
        {
            return new AuraData
            {
                AbilityId = Ability?.Id ?? NoAbilityId,
                Level = Level,
                PulseLeft = PulseLeft,
                AuraFlags = AuraFlags,
                Affects = Affects.Select(x => x.MapAffectData()).ToArray()
            };
        }

        #endregion
    }
}
