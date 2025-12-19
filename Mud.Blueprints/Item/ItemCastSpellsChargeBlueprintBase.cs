namespace Mud.Blueprints.Item;

public class ItemCastSpellsChargeBlueprintBase : ItemBlueprintBase
{
    public int SpellLevel { get; set; } // v0
    public int MaxChargeCount { get; set; } // v1
    public int CurrentChargeCount { get; set; } // v2
    public string? Spell { get; set; } // v3
    public bool AlreadyRecharged { get; set; } // v1 == 0
}
