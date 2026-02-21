using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Commands.Character.Item;
using Mud.Server.Domain.Attributes;
using Mud.Server.Guards.Interfaces;
using Mud.Server.Guards.PlayableCharacterGuards;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.Commands.PlayableCharacter;

[PlayableCharacterCommand("outfit", "Equipment", "Newbie")]
[Help(
@"The [cmd] command, usable by levels 5 and below, equips your character with
a new set of sub issue gear (banner, weapon, helmet, shield, and vest), 
courtesy of the Mayor's warehouses.  Only empty equipment slots are affected.")]
public class Outfit : WearCharacterGameActionBase<IPlayableCharacter, IPlayableCharacterGameActionInfo>
{
    protected override IGuard<IPlayableCharacter>[] Guards => [new CannotBeInCombat(), new RequiresMinPosition(Positions.Resting)];

    private IItemManager ItemManager { get; }
    private IAbilityManager AbilityManager { get; }
    private Rom24Options Options { get; }

    public Outfit(ILogger<Outfit> logger, IItemManager itemManager, IAbilityManager abilityManager, IOptions<Rom24Options> options)
        : base(logger)
    {
        ItemManager = itemManager;
        AbilityManager = abilityManager;
        Options = options.Value;
    }

    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        if (Actor.Level > 5)
            return "Find it yourself!";

        return null;
    }

    public override void Execute(IActionInput actionInput)
    {
        // light
        if (Actor.GetEquipment(EquipmentSlots.Light) == null)
        {
            var banner = ItemManager.AddItem<IItemLight>(Guid.NewGuid(), Options.MudSchool.Banner, $"Outfit[{Actor.DebugName}]", Actor);
            if (banner != null)
                WearItem(banner, false);
        }
        // chest
        if (Actor.GetEquipment(EquipmentSlots.Chest) == null)
        {
            var vest = ItemManager.AddItem<IItemArmor>(Guid.NewGuid(), Options.MudSchool.Vest, $"Outfit[{Actor.DebugName}]", Actor);
            if (vest != null)
                WearItem(vest, false);
        }
        // weapon
        if (Actor.GetEquipment(EquipmentSlots.MainHand) == null)
        {
            // search weapon with highest learned percentage
            var currentBestPercentage = 0;
            var currentBestWeaponType = WeaponTypes.Sword; // by default
            foreach (var weaponType in Enum.GetValues<WeaponTypes>())
            {
                var weaponAbilityDefinition = AbilityManager[weaponType];
                if (weaponAbilityDefinition != null)
                {
                    var weaponAbilityName = weaponAbilityDefinition.Name;
                    var (weaponPercentage, _) = Actor.GetAbilityLearnedAndPercentage(weaponAbilityName);
                    if (weaponPercentage > currentBestPercentage)
                    {
                        currentBestPercentage = weaponPercentage;
                        currentBestWeaponType = weaponType;
                    }
                }
            }
            // get weapon id from weapon type
            var blueprintId = currentBestWeaponType switch
            {
                WeaponTypes.Sword => Options.MudSchool.Sword,
                WeaponTypes.Dagger => Options.MudSchool.Dagger,
                WeaponTypes.Spear => Options.MudSchool.Spear,
                WeaponTypes.Mace => Options.MudSchool.Mace,
                WeaponTypes.Axe => Options.MudSchool.Axe,
                WeaponTypes.Flail => Options.MudSchool.Flail,
                WeaponTypes.Whip => Options.MudSchool.Whip,
                WeaponTypes.Staff => Options.MudSchool.Staff,
                _ => Options.MudSchool.Sword
            };
            //
            var weapon = ItemManager.AddItem<IItemWeapon>(Guid.NewGuid(), blueprintId, $"Outfit[{Actor.DebugName}]", Actor);
            if (weapon != null)
                WearItem(weapon, false);
        }
        // shield
        if (Actor.GetEquipment(EquipmentSlots.OffHand) == null && Actor.GetEquipment(EquipmentSlots.MainHand) != null)
        {
            var shield = ItemManager.AddItem<IItemShield>(Guid.NewGuid(), Options.MudSchool.Shield, $"Outfit[{Actor.DebugName}]", Actor);
            if (shield != null)
                WearItem(shield, false);
        }
        //
        Actor.Send("You have been equipped by Mota.");
    }
}
