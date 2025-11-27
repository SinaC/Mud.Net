using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Table;
using Mud.Server.Options;
using System.Text;

namespace Mud.Server.Item;

[Export(typeof(IItemWeapon))]
public class ItemWeapon : ItemBase, IItemWeapon
{
    private ITableValues TableValues { get; }

    public ItemWeapon(ILogger<ItemWeapon> logger, IServiceProvider serviceProvider, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, ITableValues tableValues)
       : base(logger, serviceProvider, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager)
    {
        TableValues = tableValues;
    }

    public void Initialize(Guid guid, ItemWeaponBlueprint blueprint, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, containedInto);

        Type = blueprint.Type;
        DiceCount = blueprint.DiceCount;
        DiceValue = blueprint.DiceValue;
        DamageType = blueprint.DamageType;
        BaseWeaponFlags = NewAndCopyAndSet<IWeaponFlags, IWeaponFlagValues>(() => new WeaponFlags(ServiceProvider), blueprint.Flags, null);
        WeaponFlags = NewAndCopyAndSet<IWeaponFlags, IWeaponFlagValues>(() => new WeaponFlags(ServiceProvider), BaseWeaponFlags, null);
        DamageNoun = blueprint.DamageNoun;
    }

    public void Initialize(Guid guid, ItemWeaponBlueprint blueprint, ItemWeaponData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        Type = blueprint.Type;
        DiceCount = blueprint.DiceCount;
        DiceValue = blueprint.DiceValue;
        DamageType = blueprint.DamageType;
        BaseWeaponFlags = NewAndCopyAndSet<IWeaponFlags, IWeaponFlagValues>(() => new WeaponFlags(ServiceProvider), itemData.WeaponFlags, null);
        WeaponFlags = NewAndCopyAndSet<IWeaponFlags, IWeaponFlagValues>(() => new WeaponFlags(ServiceProvider), BaseWeaponFlags, null);
        DamageNoun = blueprint.DamageNoun;
    }

    #region IItemWeapon

    #region IItem

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemWeapon>();

    #endregion

    public override void Recompute() // overriding recompute and calling base will cause every collection to be iterate twice
    {
        Logger.LogDebug("ItemWeapon.Recompute: {name}", DebugName);

        base.Recompute();

        // 1/ Apply auras from room containing item if in a room
        if (ContainedInto is IRoom room && room.IsValid)
        {
            ApplyAuras<IItemWeapon>(ContainedInto, this);
        }

        // 2/ Apply auras from character equiping item if equipped by a character
        if (EquippedBy != null && EquippedBy.IsValid)
        {
            ApplyAuras<IItemWeapon>(EquippedBy, this);
        }

        // 3/ Apply own auras
        ApplyAuras<IItemWeapon>(this, this);
    }

    #endregion

    public WeaponTypes Type { get; private set; }
    public int DiceCount { get; private set; }
    public int DiceValue { get; private set; }
    public SchoolTypes DamageType { get; private set; }

    public IWeaponFlags BaseWeaponFlags { get; protected set; } = null!;
    public IWeaponFlags WeaponFlags { get; protected set; } = null!;

    public string DamageNoun { get; private set; } = null!;

    public bool CanWield(ICharacter character)
    {
        return TotalWeight <= TableValues.WieldBonus(character) * 10;
    }

    public void ApplyAffect(IItemWeaponFlagsAffect affect)
    {
        switch (affect.Operator)
        {
            case AffectOperators.Add:
            case AffectOperators.Or:
                WeaponFlags.Set(affect.Modifier);
                break;
            case AffectOperators.Assign:
                WeaponFlags.Copy(affect.Modifier);
                break;
            case AffectOperators.Nor:
                WeaponFlags.Unset(affect.Modifier);
                break;
        }
    }

    #endregion

    #region ItemBase

    public override StringBuilder Append(StringBuilder sb, ICharacter viewer, bool shortDisplay)
    {
        WeaponFlags.Append(sb, shortDisplay);
        return base.Append(sb, viewer, shortDisplay);
    }

    public override ItemData MapItemData()
    {
        return new ItemWeaponData
        {
            ItemId = Blueprint.Id,
            Level = Level,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags,
            WeaponFlags = BaseWeaponFlags,
            Auras = MapAuraData(),
        };
    }

    public override void ResetAttributes()
    {
        base.ResetAttributes();

        WeaponFlags.Copy(BaseWeaponFlags);
    }

    #endregion
}
