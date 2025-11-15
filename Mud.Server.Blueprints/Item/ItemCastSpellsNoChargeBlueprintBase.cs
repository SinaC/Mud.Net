namespace Mud.Server.Blueprints.Item;

public class ItemCastSpellsNoChargeBlueprintBase : ItemBlueprintBase
{
    public int SpellLevel { get; set; }
    public string Spell1 { get; set; } = default!;
    public string Spell2 { get; set; } = default!;
    public string Spell3 { get; set; } = default!;
    public string Spell4 { get; set; } = default!;
}
