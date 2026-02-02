using Mud.Common;
using Mud.Domain.SerializationData.Avatar;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Common;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Entity;
using System.Text;

namespace Mud.Server.Aura;

public class Aura : IAura
{
    private readonly List<IAffect> _affects;

    private Aura(IAbilityDefinition? abilityDefinition, IEntity source, IAuraFlags flags, int level, int pulseLeft, params IAffect?[]? affects)
    {
        IsValid = true;

        AbilityDefinition = abilityDefinition;
        Source = source;
        AuraFlags = flags;
        Level = level;
        PulseLeft = pulseLeft;
        if (AbilityDefinition is null)
            AuraFlags.Set("Inherent");
        _affects = (affects ?? Enumerable.Empty<IAffect?>()).Where(x => x != null!).Select(x => x!).ToList();
    }

    public Aura(IAbilityDefinition? abilityDefinition, IEntity source, IAuraFlags flags, int level, TimeSpan duration, params IAffect?[]? affects)
        : this(abilityDefinition, source, flags, level, Pulse.FromTimeSpan(duration), affects)
    {
    }

    public Aura(IAbilityDefinition? abilityDefinition, IEntity source, IAuraFlags flags, int level, params IAffect?[]? affects)
        : this(abilityDefinition, source, flags, level, -1, affects)
    {
        AuraFlags.Set("Permanent");
    }

    public Aura(IAffectManager affectManager, IAbilityManager abilityManager, AuraData auraData)
    {
        IsValid = true;

        // TODO: source
        AbilityDefinition = auraData.AbilityName is null
            ? null
            : abilityManager[auraData.AbilityName];
        AuraFlags = new AuraFlags(auraData.AuraFlags);
        Level = auraData.Level;
        PulseLeft = auraData.PulseLeft;
        if (AbilityDefinition is null)
            AuraFlags.Set("Inherent");
        if (PulseLeft < 0)
            AuraFlags.Set("Permanent");
        _affects = [];
        if (auraData.Affects != null)
        {
            foreach (var affectData in auraData.Affects)
            {
                var affect = affectManager.CreateInstance(affectData);
                if (affect != null)
                    _affects.Add(affect);
            }
        }
    }

    #region IAura

    public bool IsValid { get; private set; }

    public int Level { get; private set; }

    public int PulseLeft { get; private set; }

    public string AbilityName => AbilityDefinition?.Name ?? string.Empty;

    public IAbilityDefinition? AbilityDefinition { get; }

    public IEntity? Source { get; private set; }

    public IAuraFlags AuraFlags { get; }

    public IEnumerable<IAffect> Affects => _affects;

    public void Update(int newLevel, TimeSpan newDuration)
    {
        Level = newLevel;
        PulseLeft = Pulse.FromTimeSpan(newDuration);
    }

    public T? AddOrUpdateAffect<T>(Func<T, bool> filterFunc, Func<T?> createFunc, Action<T>? updateFunc)
        where T : IAffect
    {
        var affect = _affects.OfType<T>().FirstOrDefault(filterFunc);
        if (affect == null)
        {
            if (createFunc != null)
            {
                affect = createFunc();
                if (affect != null)
                {
                    _affects.Add(affect);
                }
            }
        }
        else
            updateFunc?.Invoke(affect);
        return affect;
    }

    public bool DecreasePulseLeft(int pulseCount)
    {
        if (AuraFlags.IsSet("Permanent") || PulseLeft < 0)
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
        Source = null!;
    }

    public StringBuilder Append(StringBuilder sb, bool shortDisplay = false)
        => Append<IAffect>(sb, shortDisplay);

    public StringBuilder Append<TAffect>(StringBuilder sb, bool shortDisplay = false)
        where TAffect : IAffect
    {
        // TODO // if lvl < 10: only ability
        // TODO // if lvl < 15: only ability and duration
        // TODO // if lvl >= 15: ability, duration + affects
        // TODO // if admin: ability, level, duration, flags + affects
        // TODO admin see hidden auras

        // TODO: better formatting with spacing like in score
        sb.AppendFormatLine("%B%{0,-15}%x% (lvl {1}) {2} {3}",
                AbilityNameOrInherent(),
                Level,
                AuraFlags.IsSet("Permanent")
                    ? string.Empty
                    //: $"%G%{(PulseLeft / Pulse.PulsePerSeconds).FormatDelay()}%x% left",
                    : $"%G%{Pulse.ToTimeSpan(PulseLeft).FormatDelay()}%x% left",
                AuraFlags.IsNone
                    ? string.Empty
                    : AuraFlags);
        if (!shortDisplay)
            foreach (var affect in Affects.OfType<TAffect>())
            {
                sb.Append("    ");
                affect.Append(sb);
                sb.AppendLine();
            }

        return sb;
    }

    private string AbilityNameOrInherent()
    {
        if (AuraFlags.IsSet("Inherent") || AbilityName is null)
            return "Inherent";
        return AbilityName;
    }

    public virtual AuraData MapAuraData()
    {
        return new AuraData
        {
            AbilityName = AbilityName,
            Level = Level,
            PulseLeft = PulseLeft,
            AuraFlags = AuraFlags.Serialize(),
            Affects = Affects.Select(x => x.MapAffectData()).ToArray()
        };
    }

    #endregion
}
