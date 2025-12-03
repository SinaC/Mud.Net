using Mud.Server.Flags.Interfaces;
using System.Text.Json.Serialization;

namespace Mud.Domain;

[JsonDerivedType(typeof(ItemData), "base")]
[JsonDerivedType(typeof(ItemCorpseData), "corpse")]
[JsonDerivedType(typeof(ItemContainerData), "container")]
[JsonDerivedType(typeof(ItemWeaponData), "weapon")]
[JsonDerivedType(typeof(ItemDrinkContainerData), "drinkContainer")]
[JsonDerivedType(typeof(ItemFoodData), "food")]
[JsonDerivedType(typeof(ItemPortalData), "portal")]
[JsonDerivedType(typeof(ItemWandData), "wand")]
[JsonDerivedType(typeof(ItemStaffData), "staff")]
[JsonDerivedType(typeof(ItemLightData), "light")]
public class ItemData
{
    public required int ItemId { get; set; }

    public required int Level { get; set; }

    public required int DecayPulseLeft { get; set; }

    public required IItemFlags ItemFlags { get; set; }

    public required AuraData[] Auras { get; set; }
}
