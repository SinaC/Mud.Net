using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Commands.Character.Item;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.Commands.PlayableCharacter
{
    [PlayableCharacterCommand("outfit", "Equipment", "Newbie", MinPosition = Positions.Standing, NotInCombat = true)]
    [Help(
@"The [cmd] command, usable by levels 5 and below, equips your character with
a new set of sub issue gear (banner, weapon, helmet, shield, and vest), 
courtesy of the Mayor's warehouses.  Only empty equipment slots are affected.")]
    public class Outfit : WearCharacterGameActionBase
    {
        private IItemManager ItemManager { get; }
        private IAbilityManager AbilityManager { get; }
        private Rom24Options Options { get; }

        public Outfit(IWiznet wiznet, IItemManager itemManager, IAbilityManager abilityManager, IOptions<Rom24Options> options)
            : base(wiznet)
        {
            ItemManager = itemManager;
            AbilityManager = abilityManager;
            Options = options.Value;
        }

        public override string? Guards(IActionInput actionInput)
        {
            var baseGuards = base.Guards(actionInput);
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
                var banner = ItemManager.AddItem<IItemLight>(Guid.NewGuid(), Options.MudSchool.Banner, Actor);
                if (banner != null)
                    WearItem(banner, false);
            }
            // chest
            if (Actor.GetEquipment(EquipmentSlots.Chest) == null)
            {
                var vest = ItemManager.AddItem<IItemArmor>(Guid.NewGuid(), Options.MudSchool.Vest, Actor);
                if (vest != null)
                    WearItem(vest, false);
            }
            // weapon
            if (Actor.GetEquipment(EquipmentSlots.MainHand) == null)
            {
                // search weapon with highest learned percentage
                var currentBestPercentage = 0;
                var currentBestWeaponType = WeaponTypes.Sword; // by default
                foreach (var weaponType in EnumHelpers.GetValues<WeaponTypes>())
                {
                    var weaponAbilityInfo = AbilityManager[weaponType];
                    if (weaponAbilityInfo != null)
                    {
                        var weaponAbilityName = weaponAbilityInfo.Name;
                        var (weaponPercentage, _) = Actor.GetAbilityLearnedInfo(weaponAbilityName);
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
                var weapon = ItemManager.AddItem<IItemWeapon>(Guid.NewGuid(), blueprintId, Actor);
                if (weapon != null)
                    WearItem(weapon, false);
            }
            // shield
            if (Actor.GetEquipment(EquipmentSlots.OffHand) == null && Actor.GetEquipment(EquipmentSlots.MainHand) != null)
            {
                var shield = ItemManager.AddItem<IItemShield>(Guid.NewGuid(), Options.MudSchool.Vest, Actor);
                if (shield != null)
                    WearItem(shield, false);
            }
            //
            Actor.Send("You have been equipped by Mota.");
        }
    }
}
