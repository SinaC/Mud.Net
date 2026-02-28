namespace Mud.POC.DeferredAction;

public class Room: IContainer
{
    public string Name { get; set; }

    public RoomFlags Flags { get; set; }

    public List<Mob> Mobs { get; } = new();
    public List<Item> Items { get; } = new();
    public List<Exit> Exits { get; } = new();

    public Exit GetRandomExit()
        => Exits[0];
}

public class Exit
{
    public Room TargetRoom { get; set; }
}