using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Common;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;
using System.Reflection;

namespace Mud.Server.Race;

public abstract class PlayableRaceBase : RaceBase, IPlayableRace
{
    private readonly List<IAbilityUsage> _abilities;

    private readonly List<EquipmentSlots> _basicSlots =
    [
        Domain.EquipmentSlots.Light,
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Amulet, // 2 amulets
        Domain.EquipmentSlots.Amulet,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Cloak,
        Domain.EquipmentSlots.Waist,
        Domain.EquipmentSlots.Wrists, // 2 wrists
        Domain.EquipmentSlots.Wrists,
        Domain.EquipmentSlots.Arms,
        Domain.EquipmentSlots.Hands,
        Domain.EquipmentSlots.Ring, // 2 rings
        Domain.EquipmentSlots.Ring,
        Domain.EquipmentSlots.Legs,
        Domain.EquipmentSlots.Feet,
        Domain.EquipmentSlots.MainHand, // 2 hands
        Domain.EquipmentSlots.OffHand,
        Domain.EquipmentSlots.Float,
    ];

    protected ILogger<PlayableRaceBase> Logger { get; }
    protected IAbilityManager AbilityManager { get; }

    #region IPlayableRace

    public abstract string ShortName { get; }

    public string? Help { get; }

    public IEnumerable<IAbilityUsage> Abilities => _abilities;

    public override IEnumerable<EquipmentSlots> EquipmentSlots => _basicSlots;

    public abstract int GetStartAttribute(CharacterAttributes attribute);

    public abstract int GetMaxAttribute(CharacterAttributes attribute);

    public virtual int ClassExperiencePercentageMultiplier(IClass? c) => 100;

    #endregion

    protected PlayableRaceBase(ILogger<PlayableRaceBase> logger, IFlagFactory flagFactory, IAbilityManager abilityManager)
        : base(flagFactory)
    {
        Logger = logger;
        AbilityManager = abilityManager;

        _abilities = [];
        var helpAttribute = GetType().GetCustomAttribute<HelpAttribute>();
        Help = helpAttribute?.Help;
    }

    protected void AddAbility(string abilityName)
    {
        AddAbility(1, abilityName, null, 0, CostAmountOperators.None, 1);
    }

    protected void AddAbility(int level, string abilityName, ResourceKinds? resourceKind, int costAmount, CostAmountOperators costAmountOperator, int rating)
    {
        var abilityInfo = AbilityManager[abilityName];
        if (abilityInfo == null)
        {
            Logger.LogError("Trying to add unknown ability [{abilityName}] to race [{name}]", abilityName, Name);
            return;
        }
        //
        _abilities.Add(new AbilityUsage(abilityName, level, resourceKind, costAmount, costAmountOperator, rating, 100, abilityInfo));
    }
}
