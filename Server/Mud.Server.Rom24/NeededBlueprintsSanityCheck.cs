using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.Blueprints.Item;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24;

[Export(typeof(ISanityCheck)), Shared]
public class NeededBlueprintsSanityCheck : ISanityCheck
{
    private ILogger<NeededBlueprintsSanityCheck> Logger { get; }
    private IItemManager ItemManager { get; }
    private ICharacterManager CharacterManager { get; }
    private Rom24Options Rom24Options { get; }

    public NeededBlueprintsSanityCheck(ILogger<NeededBlueprintsSanityCheck> logger, IOptions<Rom24Options> options, IItemManager itemManager, ICharacterManager characterManager)
    {
        Logger = logger;
        ItemManager = itemManager;
        CharacterManager = characterManager;
        Rom24Options = options.Value;
    }

    public bool PerformSanityChecks()
    {
        // Spells
        if (ItemManager.GetItemBlueprint<ItemFoodBlueprint>(Rom24Options.SpellBlueprintIds.Mushroom) == null)
            Logger.LogError("'a Magic mushroom' blueprint {blueprintId} not found or not food (needed for spell CreateFood)", Rom24Options.SpellBlueprintIds.Mushroom);
        if (ItemManager.GetItemBlueprint<ItemFountainBlueprint>(Rom24Options.SpellBlueprintIds.Spring) == null)
            Logger.LogError("'a magical spring' blueprint {blueprintId} not found or not a fountain (needed for spell CreateSpring)", Rom24Options.SpellBlueprintIds.Spring);
        if (ItemManager.GetItemBlueprint<ItemLightBlueprint>(Rom24Options.SpellBlueprintIds.LightBall) == null)
            Logger.LogError("'a bright ball of light' blueprint {blueprintId} not found or not a light (needed for spell ContinualLight)", Rom24Options.SpellBlueprintIds.LightBall);
        if (ItemManager.GetItemBlueprint<ItemContainerBlueprint>(Rom24Options.SpellBlueprintIds.FloatingDisc) == null)
            Logger.LogError("'a floating disc' blueprint {blueprintId} not found or not a container (needed for spell FloatingDisc)", Rom24Options.SpellBlueprintIds.FloatingDisc);
        if (ItemManager.GetItemBlueprint<ItemPortalBlueprint>(Rom24Options.SpellBlueprintIds.Portal) == null)
            Logger.LogError("'a portal' blueprint {blueprintId} not found or not a portal (needed for spells Portal and Nexus)", Rom24Options.SpellBlueprintIds.Portal);
        if (ItemManager.GetItemBlueprint<ItemTrashBlueprint>(Rom24Options.SpellBlueprintIds.Rose) == null)
            Logger.LogError("'a beautiful rose' blueprint {blueprintId} not found or not a trash (needed for spell CreateRose)", Rom24Options.SpellBlueprintIds.Rose);

        // Dangerous Neighborhood
        if (CharacterManager.GetCharacterBlueprint(Rom24Options.DangerousNeighborhood.PatrolMan) == null)
            Logger.LogError("Patrol man blueprint {blueprintId} not found", Rom24Options.DangerousNeighborhood.PatrolMan);
        if (CharacterManager.CharacterBlueprints.All(x => x.Group != Rom24Options.DangerousNeighborhood.Trolls))
            Logger.LogError("No characters found in Trolls group {group}", Rom24Options.DangerousNeighborhood.Trolls);
        if (CharacterManager.CharacterBlueprints.All(x => x.Group != Rom24Options.DangerousNeighborhood.Ogres))
            Logger.LogError("No characters found in Ogres group {group}", Rom24Options.DangerousNeighborhood.Ogres);

        // Mudschool
        if (ItemManager.GetItemBlueprint<ItemWeaponBlueprint>(Rom24Options.MudSchool.Mace) == null)
            Logger.LogError("'a sub issue mace' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Mace);
        if (ItemManager.GetItemBlueprint<ItemWeaponBlueprint>(Rom24Options.MudSchool.Dagger) == null)
            Logger.LogError("'a sub issue dagger' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Dagger);
        if (ItemManager.GetItemBlueprint<ItemWeaponBlueprint>(Rom24Options.MudSchool.Sword) == null)
            Logger.LogError("'a sub issue sword' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Sword);
        if (ItemManager.GetItemBlueprint<ItemWeaponBlueprint>(Rom24Options.MudSchool.Spear) == null)
            Logger.LogError("'a sub issue spear' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Spear);
        if (ItemManager.GetItemBlueprint<ItemWeaponBlueprint>(Rom24Options.MudSchool.Staff) == null)
            Logger.LogError("'a sub issue staff' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Staff);
        if (ItemManager.GetItemBlueprint<ItemWeaponBlueprint>(Rom24Options.MudSchool.Axe) == null)
            Logger.LogError("'a sub issue axe' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Axe);
        if (ItemManager.GetItemBlueprint<ItemWeaponBlueprint>(Rom24Options.MudSchool.Flail) == null)
            Logger.LogError("'a sub issue flail' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Flail);
        if (ItemManager.GetItemBlueprint<ItemWeaponBlueprint>(Rom24Options.MudSchool.Whip) == null)
            Logger.LogError("'a sub issue whip' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Whip);
        if (ItemManager.GetItemBlueprint<ItemWeaponBlueprint>(Rom24Options.MudSchool.Polearm) == null)
            Logger.LogError("'a sub issue polearm' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Polearm);
        if (ItemManager.GetItemBlueprint<ItemArmorBlueprint>(Rom24Options.MudSchool.Vest) == null)
            Logger.LogError("'a sub issue vest' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Vest);
        if (ItemManager.GetItemBlueprint<ItemShieldBlueprint>(Rom24Options.MudSchool.Shield) == null)
            Logger.LogError("'a sub issue shield' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Shield);
        if (ItemManager.GetItemBlueprint<ItemLightBlueprint>(Rom24Options.MudSchool.Banner) == null)
            Logger.LogError("'a sub issue banner' blueprint {blueprintId} not found or not weapon (needed for outfit command)", Rom24Options.MudSchool.Banner);

        return false;
    }
}
