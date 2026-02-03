using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Item;
using Mud.Domain;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Random;
using Mud.Server.Parser.Interfaces;
using Mud.Server.Domain.Attributes;
using Mud.Server.Domain;
using Mud.Server.Domain.SerializationData;
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
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Item;

[Item(typeof(ItemWeaponBlueprint), typeof(ItemWeaponData))]
public class ItemWeapon : ItemBase, IItemWeapon
{
    private ITableValues TableValues { get; }
    private IFlagsManager FlagsManager { get; }

    public ItemWeapon(ILogger<ItemWeapon> logger, IGameActionManager gameActionManager, IParser parser, IOptions<MessageForwardOptions> messageForwardOptions, IOptions<WorldOptions> worldOptions, IRandomManager randomManager, IAuraManager auraManager, ITableValues tableValues, IFlagsManager flagsManager)
       : base(logger, gameActionManager, parser, messageForwardOptions, worldOptions, randomManager, auraManager)
    {
        TableValues = tableValues;

        WeaponFlags = new WeaponFlags();
        FlagsManager = flagsManager;
    }

    public void Initialize(Guid guid, ItemWeaponBlueprint blueprint, string source, IContainer containedInto) 
    {
        base.Initialize(guid, blueprint, source, containedInto);

        Type = blueprint.Type;
        DamageType = blueprint.DamageType;
        BaseWeaponFlags = NewAndCopyAndSet(() => new WeaponFlags(), blueprint.Flags, null);
        DamageNoun = blueprint.DamageNoun;

        if (blueprint.ItemFlags != null && blueprint.ItemFlags.IsSet("RandomStats"))
        {
            DiceCount = blueprint.DiceCount + RandomManager.Range(-1, 1);
            DiceValue = blueprint.DiceCount + RandomManager.Range(-1, 1);
        }
        else
        {
            DiceCount = blueprint.DiceCount;
            DiceValue = blueprint.DiceValue;
        }

        ResetAttributesAndResourcesAndFlags();
    }

    public void Initialize(Guid guid, ItemWeaponBlueprint blueprint, ItemWeaponData itemData, IContainer containedInto)
    {
        base.Initialize(guid, blueprint, itemData, containedInto);

        Type = blueprint.Type;
        DiceCount = itemData.DiceCount;
        DiceValue = itemData.DiceValue;
        DamageType = blueprint.DamageType;
        BaseWeaponFlags = NewAndCopyAndSet(() => new WeaponFlags(), new WeaponFlags(itemData.WeaponFlags), null);
        DamageNoun = blueprint.DamageNoun;

        ResetAttributesAndResourcesAndFlags();
    }

    #region IItemWeapon

    #region IItem

    public override void Recompute()
    {
        Logger.LogDebug("ItemWeapon.Recompute: {name}", DebugName);

        // don't call base.Recompute because it would apply IItem aura and IItemAura would be applied again in ApplyAura<IItemWeapon>
        //base.Recompute();
        // 0) Reset
        ResetAttributesAndResourcesAndFlags();

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

    public void SetDices(int diceCount, int diceValue)
    {
        DiceCount = diceCount;
        DiceValue = diceValue;
    }

    #endregion

    #region ItemBase

    public override StringBuilder Append(StringBuilder sb, ICharacter viewer, bool shortDisplay)
    {
        FlagsManager.Append(sb, WeaponFlags, shortDisplay);
        return base.Append(sb, viewer, shortDisplay);
    }

    public override ItemWeaponData MapItemData()
    {
        return new ItemWeaponData
        {
            ItemId = Blueprint.Id,
            Source = Source,
            ShortDescription = ShortDescription,
            Description = Description,
            Level = Level,
            Cost = Cost,
            DecayPulseLeft = DecayPulseLeft,
            ItemFlags = BaseItemFlags.Serialize(),
            Auras = MapAuraData(),
            WeaponFlags = BaseWeaponFlags.Serialize(),
            DiceCount = DiceCount,
            DiceValue = DiceValue,
        };
    }

    #endregion

    protected override void ResetAttributesAndResourcesAndFlags()
    {
        base.ResetAttributesAndResourcesAndFlags();

        WeaponFlags.Copy(BaseWeaponFlags);
    }
}
