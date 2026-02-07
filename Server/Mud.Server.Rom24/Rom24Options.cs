namespace Mud.Server.Rom24;

public class Rom24Options
{
    public const string SectionName = "Rom24.Settings";

    public required SpellBlueprintIds SpellBlueprintIds { get; init; }
    public required DangerousNeighborhood DangerousNeighborhood { get; init; }
    public required MudSchool MudSchool { get; init; }
}

public class SpellBlueprintIds
{
    public required int Mushroom { get; init; }
    public required int LightBall { get; init; }
    public required int Spring { get; init; }
    public required int FloatingDisc { get; init; }
    public required int Portal { get; init; }
    public required int Rose { get; init; }
}

public class DangerousNeighborhood
{
    public required int PatrolMan { get; init; }
    public required int Trolls { get; init; }
    public required int Ogres { get; init; }
    public required int Whistle{ get; init; }
}

public class MudSchool
{
    public required int Mace { get; init; }
    public required int Dagger { get; init; }
    public required int Sword { get; init; }
    public required int Spear { get; init; }
    public required int Staff { get; init; }
    public required int Axe { get; init; }
    public required int Flail { get; init; }
    public required int Whip { get; init; }
    public required int Polearm { get; init; }

    public required int Vest { get; init; }
    public required int Shield { get; init; }
    public required int Banner { get; init; }
}
