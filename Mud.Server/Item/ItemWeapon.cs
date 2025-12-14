using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.DataStructures.Trie;
using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Item;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect.Item;
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

[Item(typeof(ItemWeaponBlueprint), typeof(ItemWeaponData))]
public class ItemWeapon : ItemBase, IItemWeapon
{
    private ITableValues TableValues { get; }
    private IFlagsManager FlagsManager { get; }

    public ItemWeapon(ILogger<ItemWeapon> logger, IGameActionManager gameActionManager, ICommandParser commandParser, IAbilityManager abilityManager, IOptions<MessageForwardOptions> messageForwardOptions, IRoomManager roomManager, IAuraManager auraManager, ITableValues tableValues, IFlagsManager flagsManager)
       : base(logger, gameActionManager, commandParser, abilityManager, messageForwardOptions, roomManager, auraManager)
    {
        TableValues = tableValues;

        WeaponFlags = new WeaponFlags();
        FlagsManager = flagsManager;
    }

    public void Initialize(Guid guid, ItemWeaponBlueprint blueprint, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, containedInto);

        Type = blueprint.Type;
        DiceCount = blueprint.DiceCount;
        DiceValue = blueprint.DiceValue;
        DamageType = blueprint.DamageType;
        BaseWeaponFlags = NewAndCopyAndSet(() => new WeaponFlags(), blueprint.Flags, null);
        DamageNoun = blueprint.DamageNoun;

        ResetAttributes();
    }

    public void Initialize(Guid guid, ItemWeaponBlueprint blueprint, ItemWeaponData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        Type = blueprint.Type;
        DiceCount = blueprint.DiceCount;
        DiceValue = blueprint.DiceValue;
        DamageType = blueprint.DamageType;
        BaseWeaponFlags = NewAndCopyAndSet(() => new WeaponFlags(), new WeaponFlags(itemData.WeaponFlags), null);
        DamageNoun = blueprint.DamageNoun;

        ResetAttributes();
    }

    #region IItemWeapon

    #region IItem

    #region IActor

    public override IReadOnlyTrie<IGameActionInfo> GameActions => GameActionManager.GetGameActions<ItemWeapon>();

    #endregion

    public override void Recompute()
    {
        Logger.LogDebug("ItemWeapon.Recompute: {name}", DebugName);

        // don't call base.Recompute because it would apply IItem aura and IItemAura would be applied again in ApplyAura<IItemWeapon>
        //base.Recompute();

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
        FlagsManager.Append(sb, WeaponFlags, shortDisplay);
        return base.Append(sb, viewer, shortDisplay);
    }

    public override ItemData MapItemData()
    {
        return new ItemWeaponData
        {
            ItemId = Blueprint.Id,
            Level = Level,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(),
            WeaponFlags = BaseWeaponFlags.Serialize(),
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
